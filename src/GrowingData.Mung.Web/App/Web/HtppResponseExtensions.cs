using System;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using System.IO;

using Microsoft.AspNet.Http;

namespace GrowingData.Mung.Web {
	public static class HttpResponseExtenstions {
		public static void Write(this HttpResponse resp, string content) {
			using (var writer = new StreamWriter(resp.Body)) {
				writer.Write(content);
			}
		}

		public static void AllowCors(this HttpResponse resp) {
			resp.Headers.Add("Access-Control-Allow-Origin", "*");
		}


	}
}
