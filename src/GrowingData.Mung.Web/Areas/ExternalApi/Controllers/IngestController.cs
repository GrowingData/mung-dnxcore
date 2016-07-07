using System;
using Microsoft.AspNetCore.Mvc;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;
using System.IO;
using System.Collections.Generic;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GrowingData.Mung.Web.Areas.ExternalApi.Controllers {

	public class HttpIngestController : Controller {
		[HttpPost]
		[Route("ingest/v1/events")]
		public IActionResult Ingest() {
			var appKey = Request.Query["appKey"];
			JWT.JsonWebToken.JsonSerializer = JwtHelper.Serializer;

			var app = App.Get(appKey);
			using (var reader = new StreamReader(Request.Body)) {
				var tokenString = reader.ReadToEnd();

				try {
					string jsonPayload = JWT.JsonWebToken.Decode(tokenString, app.AppSecret);
					Console.WriteLine(jsonPayload);

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
