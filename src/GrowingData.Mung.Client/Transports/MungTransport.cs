using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowingData.Mung.Client {
	internal abstract class MungTransport {
		protected string _AppKey;
		protected string _AppSecret;
		protected string _Host;
		public MungTransport(string host, string appKey, string appSecret) {
			_Host = host;
			_AppKey = appKey;
			_AppSecret = appSecret;

		}

		public abstract bool Send(IEnumerable<MungEvent> events);

	}
}
