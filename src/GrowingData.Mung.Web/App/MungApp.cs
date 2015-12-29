using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using GrowingData.Mung.Core;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Relationizer;

namespace GrowingData.Mung.Web {
	public class MungApp {
		private static MungApp _app;
		public static MungApp App {
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
		private IHostingEnvironment _env;
		private string _dataPath;
		private string _basePath;

		public EventPipeline Pipeline { get { return _pipeline; } }

		public string DataPath {
			get {
				return _dataPath;
		}
		}


		public MungApp(IHostingEnvironment env) {
			_pipeline = new EventPipeline();
			_env = env;

			_basePath = new DirectoryInfo(env.WebRootPath).Parent.FullName;
			

			_dataPath = Path.Combine(_basePath, "data");

			if (!Directory.Exists(_dataPath)) {
				Directory.CreateDirectory(_dataPath);
			}


			_pipeline.AddProcessor(new RelationalEventProcessor(_dataPath));
		}


	}
}