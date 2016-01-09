﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using GrowingData.Mung.Core;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Relationizer;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.SqlBatch;
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
		}

		private EventPipeline _pipeline;
		private LongPollingProcessor _poller;
		public EventPipeline Pipeline { get { return _pipeline; } }
		public LongPollingProcessor Poller { get { return _poller; } }


		private IHostingEnvironment _env;
		private string _dataPath;
		private string _basePath;


		public string DataPath {
			get {
				return _dataPath;
			}
		}


		public MungApp(IHostingEnvironment env) {
			Console.WriteLine("Paused Initializing of MungApp, press a key to continue...");
			Console.ReadKey();


			_pipeline = new EventPipeline();
			_poller = new LongPollingProcessor();

			_env = env;

			_basePath = new DirectoryInfo(env.WebRootPath).Parent.FullName;


			_dataPath = Path.Combine(_basePath, "data");

			if (!Directory.Exists(_dataPath)) {
				Directory.CreateDirectory(_dataPath);
			}


			_pipeline.AddProcessor(new RelationalEventProcessor(_dataPath));
			_pipeline.AddProcessor(_poller);


			// Set the serializer for our JWT
			JwtHelper.InitializeJsonSerialization();

			// Check to see if we have an "apps" yet, if not create one
			App.InitializeApps();

			// Make sure that we have a reference to the Mung Events connection
			Connection.InitializeConnection();


			BackgroundWorker.Start(new TimeSpan(0, 1, 0), () => {
				try {
					Func<NpgsqlConnection> pg = () => { return DatabaseContext.Db.Events() as NpgsqlConnection; };

					SqlBatchChecker.Check(_dataPath, pg);
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
				}
			});
		}


	}
}