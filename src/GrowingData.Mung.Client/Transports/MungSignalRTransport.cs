using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowingData.Mung.Client {
	internal class MungSignalRTransport : MungTransport {

		

		public MungSignalRTransport(string host, string appKey, string appSecret) :
			base(host, appKey, appSecret) {
			_Host = host;
			_AppKey = appKey;
			_AppSecret = appSecret;

		}

		public override bool Send(IEnumerable<MungEvent> events) {
			return false;
		}

	}
}
