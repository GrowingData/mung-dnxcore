using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GrowingData.Mung.Client {
	internal class PersistentQueueEvent {
		public MungEvent Event;
		public string FileName;
		public long FilePosition;
		public int RetryCount = 0;
	}

	internal class PersistentQueue {
		private object _fileLock = new object();

		private string _basePath;
		private MungTransport _connection;
		private ConcurrentQueue<PersistentQueueEvent> _eventQueue;
		private Dictionary<string, PersistentFileHandle> _files;

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
			_files = new Dictionary<string, PersistentFileHandle>();

			Task.Run(() => ProcessQueue());
			//Task.Run(() => CheckOldOpenFiles());
		}

		public static string GetFilename(string name) {

			var fileName = string.Format("{0}-{1}.log",
				name,
				DateTime.UtcNow.ToString("yyyy-MM-dd.HH")
			);

			return fileName;
		}
		public static string GetFilename() {
			return GetFilename("mung-events");
		}


		public void Send(MungEvent evt) {
			var wrapped = new PersistentQueueEvent() {
				Event = evt,
				FileName = Path.Combine(_basePath, GetFilename()),
				FilePosition = -1
			};
			// Write it to our log
			WriteEvent(wrapped);

			// Then write it to the actual queue.
			_eventQueue.Enqueue(wrapped);

		}

		public void WriteEvent(PersistentQueueEvent evt) {
			PersistentFileHandle handle = GetFileHandleLocked(evt);
			handle.WriteEvent(evt);
		}

		public void SetEventComplete(PersistentQueueEvent evt) {
			PersistentFileHandle handle = GetFileHandleLocked(evt);
			handle.SetEventComplete(evt);
		}

		private PersistentFileHandle GetFileHandleLocked(PersistentQueueEvent evt) {
			PersistentFileHandle handle = null;
			lock (_fileLock) {
				if (_files.ContainsKey(evt.FileName)) {
					handle = _files[evt.FileName];
				} else {
					handle = new PersistentFileHandle(evt.FileName);
					_files[evt.FileName] = handle;
				}
			}
			return handle;
		}

		public void WaitUntilQueueEmpty() {
			while (_eventQueue.Count > 0) {
				Thread.Sleep(10);
			}
		}


		private void CheckOldOpenFiles() {

			while (true) {
				var currentFile = GetFilename();
				var toRemove = new List<string>();
				foreach (var k in _files.Keys) {
					if (k != currentFile) {
						toRemove.Add(k);
					}
				}
				lock (_fileLock) {
					foreach (var key in toRemove) {
						_files.Remove(key);
					}
				}


				Thread.Sleep(1000 * 60);
			}


		}

		private void ProcessQueue() {
			Console.WriteLine("Processing queue on thread: " + Thread.CurrentThread.ManagedThreadId);
			while (true) {
				PersistentQueueEvent msg;

				var batch = new List<PersistentQueueEvent>();
				while (_eventQueue.TryDequeue(out msg) && batch.Count < 32) {
					batch.Add(msg);
				}
				if (batch.Count > 0) {
					try {
						Console.WriteLine("Sending batch of: " + batch.Count.ToString() + " events:");

						if (_connection.Send(batch.Select(x => x.Event))) {
							// Mark these events as having been sent
							foreach (var evt in batch) {
								SetEventComplete(evt);
							}
							Console.WriteLine("Batch sent and marked complete.");
						} else {
							foreach (var evt in batch) {
								_eventQueue.Enqueue(evt);
							}
							Console.WriteLine("Batch send failed, re-queuing for later");
						}

					} catch (Exception ex) {
						Console.WriteLine(string.Format("MUNG: Unable to send event: {0}", ex.Message));
					}
				}
				Thread.Sleep(100);
			}
		}

	}
}

