using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Rendering;
using GrowingData.Mung;
using GrowingData.Utilities;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GrowingData.Mung.Web.Models;
using GrowingData.Mung.Core;
using GrowingData.Utilities.DnxCore;
using System.IO;


namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class QueryController : MungSecureController {
		// GET: Dashboards/CreateDashboard

		public QueryController(IHostingEnvironment env) : base(env) {
		}

		[Route("queries")]
		public ActionResult QueryList(string url) {

			ViewBag.Queries = Query.List(CurrentUser.MungerId);
			return View("QueryList");
		}
		

		[Route("re-relationize")]
		public ActionResult Migrate(string eventType, DateTime since) {

			using (var cn = DatabaseContext.Db.Events()) {
				var sql = @"SELECT * FROM mung.all WHERE event_type = @eventType AND at > @since";
				using (var reader = cn.DumpReader(sql, new { eventType = eventType, since = since })) {
					if (!reader.Read()) {
						return Json(new {
							Success = false,
							Message = $"The event type \"{eventType}\" does not yet exist.  Fire off a test event to enable live preview.",
							highlight = "mung-event-type"
						});
					}
					while (reader.Read()) {
						MungApp.Current.RelationalEventProcessor.EnqueueEvent(new MungServerEvent() {
							LogTime = (DateTime)reader["at"],
							Source = (string)reader["source"],
							AppId = (int)reader["app_id"],
							Type = (string)reader["event_type"],
							Data = JToken.Parse((string)reader["json_data"])
						});
					}
				}
			}

			return Content("Success!");
		}
	}
}