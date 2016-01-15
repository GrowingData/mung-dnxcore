using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;


namespace GrowingData.Mung.Client {
	public class MungClient {
		private MungTransport _connection;
		private PersistentQueue _queue;


		public MungClient() {

			var appHost = ConfigurationManager.AppSettings["mung-host"];
			var appKey = ConfigurationManager.AppSettings["mung-key"];
			var appSecret = ConfigurationManager.AppSettings["mung-secret"];

			_connection = new MungJwtTransport(appHost, appKey, appSecret);

			_queue = new PersistentQueue(_connection);

		}


		public void WaitUntilQueueEmpty() {
			_queue.WaitUntilQueueEmpty();
		}

		public void Send(string type, dynamic data) {
			var evt = new MungEvent() {
				Source = Environment.MachineName,
				Data = data,
				Type = type,
				LogTime = DateTime.UtcNow,

			};

			_queue.Send(evt);

		}

	}
}