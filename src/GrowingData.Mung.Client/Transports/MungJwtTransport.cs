using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using JWT;

namespace GrowingData.Mung.Client {
	internal class MungJwtTransport : MungTransport {
		public MungJwtTransport(string host, string appKey, string appSecret) :
		base(host, appKey, appSecret) {
			_Host = host;
			_AppKey = appKey;
			_AppSecret = appSecret;

		}

		public override bool Send(IEnumerable<MungEvent> events) {
			var payload = new Dictionary<string, object>() {
				{"events", events.ToArray() },
				{"clientTime", DateTime.UtcNow }
			};
			string token = JWT.JsonWebToken.Encode(payload, _AppSecret, JwtHashAlgorithm.HS256);
			return SendRequest(token);

		}



		private bool SendRequest(string jwt) {
		
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jwt);

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_Host + "/ingest/v1/events?appKey=" + _AppKey);
			req.Method = "POST";

			// Maximum 25 seconds
			req.Timeout = 25000;
			req.KeepAlive = true;
			req.ContentLength = bytes.Length;
			using (Stream os = req.GetRequestStream()) {
				os.Write(bytes, 0, bytes.Length); //Push it out there
				os.Close();
			}
			try {
				using (WebResponse resp = req.GetResponse()) {

					if (resp == null)
						return false;

					using (StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream())) {

						string responseString = sr.ReadToEnd().Trim();
						if (responseString == "Success") {
							return true;
						} else {
							return false;
						}
					}
				}

			} catch (WebException ex) {

				string errorText = null;
				try {
					// Try to get some more information from the error
					using (var dataStream = new StreamReader(ex.Response.GetResponseStream())) {
						errorText = dataStream.ReadToEnd();
					}

				} catch {
					// Ignore an exception trying to get the data
				}
				

				return false;
			}

		}

	}
}
