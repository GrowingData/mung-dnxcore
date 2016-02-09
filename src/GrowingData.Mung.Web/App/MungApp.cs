using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using GrowingData.Mung.Core;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Relationizer;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.SqlBatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace GrowingData.Mung.Web {
	public class MungApp {
		private static MungApp _app;
		public static MungApp Current {
			get {
				if (_app == null) {
					throw new InvalidOperationException("Please call MungApp.Initialize before trying to access the App");
				}
				return _app;
			}
		}

		public static void Initialize(IHostingEnvironment env) {
			_app = new MungApp(env);


			_app.ProcessInternalEvent("mung_init", new {
				environment = env.EnvironmentName,
				www_root = env.WebRootPath
			});

		}

		private static IViewEngine _viewEngine;

		private EventPipeline _pipeline;
		private LongPollingProcessor _longPollProcessor;
		private NotificationProcessor _notificationsProcessor;
		private JsonPostgresProcessor _jsonPostgresProcessor;
		private RelationalEventProcessor _relationalEventProcessor;
		private QueryEventProcessor _queryEventProcessor;

		public EventPipeline Pipeline { get { return _pipeline; } }
		public LongPollingProcessor LongPollProcessor { get { return _longPollProcessor; } }
		public NotificationProcessor NotificationProcessor { get { return _notificationsProcessor; } }
		public JsonPostgresProcessor JsonPostgresProcessor { get { return _jsonPostgresProcessor; } }
		public RelationalEventProcessor RelationalEventProcessor { get { return _relationalEventProcessor; } }
		public QueryEventProcessor QueryEventProcessor { get { return _queryEventProcessor; } }

		private IHostingEnvironment _env;
		private string _dataPath;
		private string _basePath;


		public string DataPath {
			get {
				return _dataPath;
			}
		}


		public void ProcessInternalEvent(string type, object data) {
			var json = JsonConvert.SerializeObject(data);

			var evt = new MungServerEvent() {
				Type = type,
				AppId = App.MungInternal.AppId,
				LogTime = DateTime.UtcNow,
				Source = System.Net.Dns.GetHostName(),
				Data = JToken.Parse(json)
			};
			_pipeline.Process(evt);
		}

		public MungApp(IHostingEnvironment env) {
			//Console.WriteLine("Paused Initializing of MungApp, press a key to continue...");
			//Console.ReadKey();


			_pipeline = new EventPipeline();

			_env = env;

			_basePath = new DirectoryInfo(env.WebRootPath).Parent.FullName;

			_dataPath = Path.Combine(_basePath, "data");

			if (!Directory.Exists(_dataPath)) {
				Directory.CreateDirectory(_dataPath);
			}

			_relationalEventProcessor = new RelationalEventProcessor(_dataPath);
			_notificationsProcessor = new NotificationProcessor();
			_longPollProcessor = new LongPollingProcessor();
			_jsonPostgresProcessor = new JsonPostgresProcessor(() => DatabaseContext.Db.Events() as NpgsqlConnection);
			_queryEventProcessor = new QueryEventProcessor();

			_pipeline.AddProcessor(_relationalEventProcessor);
			_pipeline.AddProcessor(_longPollProcessor);
			_pipeline.AddProcessor(_notificationsProcessor);
			_pipeline.AddProcessor(_jsonPostgresProcessor);
			_pipeline.AddProcessor(_queryEventProcessor);


			// Make sure that we have loaded all the settings the app needs
			Setting.InitializeSettings();


			// Check to see if we have an "apps" yet, if not create one
			App.InitializeApps();


			// Set the serializer for our JWT
			JwtHelper.InitializeJsonSerialization();

			// Make sure that we have a reference to the Mung Events connection
			Connection.InitializeConnection();


			ProcessInternalEvent("mung_init", new {
				message = "Pipelines initialized",
				data_path = _dataPath,
				base_path = _basePath
			});

			Func<NpgsqlConnection> pg = () => { return DatabaseContext.Db.Events() as NpgsqlConnection; };

			// Do the clean up synchronously so that it wont spark a race condition
			// with cleaning up old files.
			SqlBatchChecker.CleanUpOldFiles(_dataPath, pg);

			BackgroundWorker.Start(new TimeSpan(0, 1, 0), () => {
				try {

					SqlBatchChecker.Check(_dataPath, pg);
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
				}
			});

			BackgroundWorker.Start(new TimeSpan(0, 0, 0, 60), () => {
				// Fire an event each minute to be used for running periodic EventProcessors
				var now = DateTime.UtcNow;
				ProcessInternalEvent("clock_tick", new {
					Year = now.Year,
					Month = now.Month,
					Day = now.Day,
					Hour = now.Year,
					DayOfWeek = now.DayOfWeek.ToString(),
					DayOfYear = now.DayOfYear,
					Minute = now.Minute,
					Second = 0
				});
			});

		}


	}
}