using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GrowingData.Mung.Client {
	internal class PersistentQueueEvent {
		public MungEvent Event;
		public int RetryCount = 0;
	}

	internal class PersistentQueue {
		private object _fileLock = new object();
		private const string FailedBatchPrefix = "failed-batch";

		private string _basePath;
		private MungTransport _connection;
		private ConcurrentQueue<PersistentQueueEvent> _eventQueue;

		internal PersistentQueue(MungTransport connection) {
			_connection = connection;
			_eventQueue = new ConcurrentQueue<PersistentQueueEvent>();
			var pi = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while (pi.Name.ToLower() == "bin" || pi.Name.ToLower() == "debug" || pi.Name.ToLower() == "release") {
				pi = pi.Parent;
			}

			var logPath = Path.Combine(pi.FullName, "logs");
			if (!Directory.Exists(logPath)) {
				Directory.CreateDirectory(logPath);
			}
			_basePath = logPath;

			Task.Run(() => ProcessQueue());
			Task.Run(() => CheckFailedEvents());
			//Task.Run(() => CheckOldOpenFiles());
		}

		public static string GetErrorFilename() {
			var fileName = string.Format("{0}-{1}-{2}.log",
				FailedBatchPrefix,
				DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss"),
				RandomString.Get(6)
			);

			return fileName;
		}


		public void Send(MungEvent evt) {
			var wrapped = new PersistentQueueEvent() {
				Event = evt,
				RetryCount = 0
			};


			// Then write it to the actual queue.
			_eventQueue.Enqueue(wrapped);

		}

		public void WaitUntilQueueEmpty() {
			while (_eventQueue.Count > 0) {
				Thread.Sleep(100);
			}
		}
		private void CheckFailedEvents() {
			while (true) {
				Thread.Sleep(60 * 1000);

				try {
					var failedFiles = Directory.GetFiles(_basePath, FailedBatchPrefix + "*");
					foreach (var file in failedFiles) {
						Debug.WriteLine("Found failed events at: " + _basePath);

						var json = File.ReadAllText(file);

						List<PersistentQueueEvent> retryEvents = JsonConvert.DeserializeObject<List<PersistentQueueEvent>>(json);


						foreach (var evt in retryEvents) {
							evt.RetryCount = 0;
							_eventQueue.Enqueue(evt);
						}
						File.Delete(file);

						Debug.WriteLine("Queued old events and deleted: " + _basePath);
					}
				} catch (Exception ex) {

				}
			}
		}

		private void ProcessQueue() {
			Debug.WriteLine("Processing queue on thread: " + Thread.CurrentThread.ManagedThreadId);
			while (true) {
				PersistentQueueEvent msg;

				var batch = new List<PersistentQueueEvent>();
				while (_eventQueue.TryDequeue(out msg) && batch.Count < 32) {
					batch.Add(msg);
				}
				if (batch.Count > 0) {
					try {
						Debug.WriteLine("Sending batch of: " + batch.Count.ToString() + " events:");

						if (_connection.Send(batch.Select(x => x.Event))) {
							Debug.WriteLine(string.Format("Sent batch of {0} events", batch.Count));
						} else {
							var retry = batch.Where(e => e.RetryCount < 10).ToList();
							var failed = batch.Where(e => e.RetryCount >= 10).ToList();
							if (retry.Count > 0) {
								foreach (var evt in retry) {
									evt.RetryCount++;
									_eventQueue.Enqueue(evt);
								}
								Debug.WriteLine(string.Format("Batch send failed, requeued {0} events", retry.Count));
							}

							if (failed.Count > 0) {
								foreach (var evt in retry) {
									evt.RetryCount++;
								}
								var failedJson = JsonConvert.SerializeObject(failed);
								var failedPath = Path.Combine(_basePath, GetErrorFilename());

								File.WriteAllText(failedPath, failedJson);
								Debug.WriteLine(string.Format("Batch send failed, wrote {0} events to file {1}", retry.Count, failedPath));
							}

						}

					} catch (Exception ex) {
						// Write the 
						Debug.WriteLine(string.Format("MUNG: Unable to send event: {0}", ex.Message));
					}
				}
				Thread.Sleep(100);
			}
		}

	}
}

