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
		private ConcurrentQueue<MungEvent> _eventQueue;
		private string _serverUrl;
		private string _connectionUrl;

		public MungClient() {
			var appHost = ConfigurationSettings.AppSettings["mung-host"];
			var appKey = ConfigurationSettings.AppSettings["mung-key"];
			var appSecret = ConfigurationSettings.AppSettings["mung-secret"];

			_connection = new MungJwtTransport(appHost, appKey, appSecret);

			_eventQueue = new ConcurrentQueue<MungEvent>();
			Task.Run(() => ProcessQueue());
		}

		private void ProcessQueue() {
			while (true) {
				MungEvent msg;

				var batch = new List<MungEvent>();
				while (_eventQueue.TryDequeue(out msg) && batch.Count < 32) {
					batch.Add(msg);
				}
				if (batch.Count > 0) {
					try {
						Console.WriteLine("Sending batch of: " + batch.Count.ToString() + " events:");
						Console.WriteLine(JsonConvert.SerializeObject(batch, Formatting.Indented));
						_connection.Send(batch);
						Console.WriteLine("");
					} catch (Exception ex) {
						System.Diagnostics.Debug.WriteLine(string.Format("MUNG: Unable to send event: {0}", ex.Message));
					}
				}
				Thread.Sleep(100);
			}
		}

		public void WaitUntilQueueEmpty() {
			while (_eventQueue.Count > 0) {
				Thread.Sleep(10);
			}
		}

		public void Send(string source, string type, dynamic data) {
			_eventQueue.Enqueue(new MungEvent() {
				Source = source,
				Data = data,
				Type = type,
				LogTime = DateTime.UtcNow,

			});

			// Make sure the queue doesn't get too long and break everything
			// by crashing the app, so remove old entries
			MungEvent msg;
			while (_eventQueue.Count > 1000) {
				_eventQueue.TryDequeue(out msg);

			}
		}

		public void SendDirect(string source, string type, dynamic data) {
			var evt = new MungEvent() {
				Data = data,
				Type = type,
				LogTime = DateTime.UtcNow,

			};
			_connection.Send(new List<MungEvent>() { evt });
		}
	}
}