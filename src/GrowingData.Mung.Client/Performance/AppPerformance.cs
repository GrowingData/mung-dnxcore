using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace GrowingData.Mung.Client {
	public class SystemPerformance {

		private Dictionary<string, PerformanceCounter> _counters;
		private Dictionary<string, CounterSample> _lastSample;
		private CancellationTokenSource _token;
		private MungClient _client;

		public SystemPerformance() {
			_client = new MungClient();

			_counters = new Dictionary<string, PerformanceCounter>();

			_counters.Add("cpu_percent", new PerformanceCounter("Processor", "% Processor Time", "_Total"));
			_counters.Add("ram_available", new PerformanceCounter("Memory", "Available MBytes", ""));
			_counters.Add("ram_percent", new PerformanceCounter("Memory", "% Committed Bytes In Use", ""));
			_counters.Add("disk_queue", new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total"));
			_counters.Add("disk_read_mb", new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/Sec", "_Total"));
			_counters.Add("disk_write_mb", new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/Sec", "_Total"));


		}

		public void BeginCollection(TimeSpan each) {
			if (_token != null && !_token.IsCancellationRequested) {
				throw new InvalidOperationException("Already collecting data!");

			}
			_token = BackgroundWorker.Start(each, () => {
				bool hasPreviousSamples = true;


				var currentSample = new Dictionary<string, CounterSample>();
				foreach (var key in _counters.Keys) {
					currentSample[key] = _counters[key].NextSample();
				}


				if (_lastSample != null) {
					var eventData = new {
						cpu_percent = CounterSample.Calculate(_lastSample["cpu_percent"], currentSample["cpu_percent"]),
						ram_available = CounterSample.Calculate(_lastSample["ram_available"], currentSample["ram_available"]),
						ram_percent = CounterSample.Calculate(_lastSample["ram_percent"], currentSample["ram_percent"]),
						disk_queue = CounterSample.Calculate(_lastSample["disk_queue"], currentSample["disk_queue"]),
						disk_read_mb = (1.0d / (1024 * 1024)) * CounterSample.Calculate(_lastSample["disk_read_mb"], currentSample["disk_read_mb"]),
						disk_write_mb = (1.0d / (1024 * 1024)) * CounterSample.Calculate(_lastSample["disk_write_mb"], currentSample["disk_write_mb"])
					};

					_client.Send("system_performance", eventData);

				} else {
					_lastSample = new Dictionary<string, CounterSample>();
				}

				foreach (var key in _counters.Keys) {
					_lastSample[key] = _counters[key].NextSample();
				}
			});
		}

		public void StopCollection(TimeSpan each) {
			_token.Cancel();
		}



	}
}
