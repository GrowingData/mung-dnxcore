using System;
using Microsoft.AspNet.Mvc;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GrowingData.Mung.Web.Areas.Ingest.Controllers {
	public class HttpIngestController : Controller {
		[HttpPost]
		[Route("ingest/v1/events")]
		public IActionResult Ingest() {
			var appKey = Request.Query["appKey"];

			var app = App.Get(appKey);
			using (var reader = new StreamReader(Request.Body)) {
				var tokenString = reader.ReadToEnd();

				try {
					string jsonPayload = JWT.JsonWebToken.Decode(tokenString, app.AppSecret);

					var obj = JToken.Parse(jsonPayload);
					var events = obj["events"].ToObject<List<MungServerEvent>>();

					foreach (var evt in events) {
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
