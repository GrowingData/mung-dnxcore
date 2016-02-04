using System;
using Microsoft.AspNet.Mvc;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;
using System.IO;
using System.Collections.Generic;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GrowingData.Mung.Web.Areas.Ingest.Controllers {
	public class NewtonsoftJsonSerializer : IJsonSerializer {
		public static NewtonsoftJsonSerializer Instance = new NewtonsoftJsonSerializer();

		public string Serialize(object obj) {
			// Implement using favorite JSON Serializer
			return JsonConvert.SerializeObject(obj);
		}

		public T Deserialize<T>(string json) {
			// Implement using favorite JSON Serializer
			return JsonConvert.DeserializeObject<T>(json);
		}
	}

	public class HttpIngestController : Controller {
		[HttpPost]
		[Route("ingest/v1/events")]
		public IActionResult Ingest() {
			var appKey = Request.Query["appKey"];

			JWT.JsonWebToken.JsonSerializer = NewtonsoftJsonSerializer.Instance;

			var app = App.Get(appKey);
			using (var reader = new StreamReader(Request.Body)) {
				var tokenString = reader.ReadToEnd();

				try {
					string jsonPayload = JWT.JsonWebToken.Decode(tokenString, app.AppSecret);

					var obj = JToken.Parse(jsonPayload);
					var events = obj["events"].ToObject<List<MungServerEvent>>();

					foreach (var evt in events) {
						evt.AppId = app.AppId;

						MungApp.Current.Pipeline.Process(evt);
					}

					Console.WriteLine($"Got {events.Count} events");
				} catch (JWT.SignatureVerificationException) {
					Console.WriteLine("Invalid token!");
				}

			}


			return Content("Success");
		}
	}
}
