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


		[Route("migrate")]
		public ActionResult Migrate(string url) {
			Response.ContentType = "text/plain";
			var connections = Connection.List();
			var oldMung = connections.FirstOrDefault(c => c.Name == "gooroo_log");
			var newMung = connections.FirstOrDefault(c => c.Name == "mung_events");
			var sql = @"SELECT 'SELECT ' + mung.column_list(t.object_id) + ' FROM dyn.[' + t.name + ']' AS query, t.name as name
FROM sys.tables t
WHERE schema_id=5
ORDER BY t.name";

			string error = null;
			try {

				using (var writer = new StreamWriter(Response.Body)) {
					oldMung.Execute(sql, null, (row) => {
						var name = row["name"].ToString();
						var query = row["query"].ToString();
						var schema = "mung";

						newMung.Execute($"DROP TABLE IF EXISTS \"{schema}\".\"{name}\";", null, null);
						writer.WriteLine($"Copying {name}...");
						Console.WriteLine($"Copying {name}...");

						writer.Flush();
						Response.Body.Flush();


						var rowCount = 0;

						try {
							oldMung.BulkInsertTo(query, newMung, schema, name, null, (copy) => {
								rowCount++;
								if (rowCount % 1000 == 0) {
									writer.WriteLine($"\t{rowCount} rows...");
									Console.WriteLine($"\t{rowCount} rows...");
								}
							});
						} catch (Exception exx) {
							error = $"\tBulk Insert Exception:\r\n\t\t{exx.Message}\r\n\r\n{exx.StackTrace}\r\n\r\n{query}";
							throw new Exception(error);
						}
						writer.WriteLine($"Copy of {name} Complete!");
						Console.WriteLine($"Copy of {name} Complete!");
						writer.WriteLine("");

						writer.Flush();
						Response.Body.Flush();
					});

				}
				return new EmptyResult();
			} catch (Exception ex) {
				return Content($"Exception!\r\n{ex.Message}\r\n\r\n{ex.StackTrace}");
			}

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