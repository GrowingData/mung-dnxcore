using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using GrowingData.Mung.Core;

namespace GrowingData.Mung.Web {
	public class LongPollRequest : IDisposable {
		public HashSet<string> _eventTypes;
		private MungServerEvent _event;
		private AutoResetEvent _reset;
		private ConcurrentDictionary<Guid, LongPollRequest> _requestManager;
		private Guid _identifier;

		internal LongPollRequest(ConcurrentDictionary<Guid, LongPollRequest> requestManager, IEnumerable<string> eventTypes) {
			_eventTypes = new HashSet<string>(eventTypes);
			_event = null;
			_requestManager = requestManager;
			_identifier = Guid.NewGuid();

			_reset = new AutoResetEvent(false);

			_requestManager[_identifier] = this;

		}

		public void ProcessEvent(MungServerEvent mungEvent) {
			if (_eventTypes.Contains("*") || _eventTypes.Contains(mungEvent.Type)) {
				_event = mungEvent;
				_reset.Set();
			}


		}

		public MungServerEvent WaitForEvent(int timeoutMS) {
			_reset.WaitOne(timeoutMS);
			return _event;
		}

		public void Dispose() {
			LongPollRequest o;
			while (!_requestManager.TryRemove(_identifier, out o)) {
				Thread.Sleep(10);
			}
		}
	}
}
