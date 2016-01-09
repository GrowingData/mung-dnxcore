using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using GrowingData.Mung.Core;

namespace GrowingData.Mung.Web {


	public class LongPollingProcessor : EventProcessor {

		private ConcurrentDictionary<Guid, LongPollRequest> _activeRequests;


		public LongPollingProcessor() : base("LongPoller") {
			_activeRequests = new ConcurrentDictionary<Guid, LongPollRequest>();
		}

		public LongPollRequest Listen(IEnumerable<string> eventTypes) {
			var req = new LongPollRequest(_activeRequests, eventTypes);
			return req;
		}

		protected override void ProcessEvent(MungServerEvent mungEvent) {
			foreach (var longPollRequest in _activeRequests.Values) {
				longPollRequest.ProcessEvent(mungEvent);
			}


		}



	}
}
