using System;
using System.Data.Common;
using System.Linq;
using Microsoft.AspNet.Mvc;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.Database;
using System.IO;
using System.Collections.Generic;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GrowingData.Mung.Web.Areas.ExternalApi.Controllers {

	public class QueryController : ExternalApiController {

		public DbConnection GetConnection(int? connectionId) {
			if (connectionId.HasValue && connectionId.Value != -1) {
				var connection = Connection.Get(connectionId.Value);
				return connection.GetConnection();
			} else {
				return DatabaseContext.Db.Events();
			}
		}


		[Route("ext/v1/query/sql")]
		[HttpPost]
		public ActionResult Sql(string appKey, string accessToken) {
			if (!VerifyToken(appKey, accessToken)) {
				return new ApiResult(new {
					Success = false,
					Message = "Invalid token"
				});
			}


			var sql = Request.Form["sql"].ToString().Trim();
			if (!sql.ToUpper().StartsWith("SELECT") || sql.Contains(";")) {
				return new ApiResult(new {
					Success = false,
					Message = "Only SELECT queries are allowed"
				});

			}

			int? connectionId = new int?();
			var connectionIdString = Request.Query["connectionId"];
			if (!string.IsNullOrEmpty(connectionIdString)) {
				int d;
				if (int.TryParse(connectionIdString, out d)) {
					connectionId = d;
				}
			}



			Response.ContentType = "application/json";
			try {
				using (var cn = GetConnection(connectionId)) {
					return new ApiResult(new {
						Success = true,
						Rows = cn.SelectRowsList(sql, null)
					});

				}
			} catch (Exception ex) {
				return new ApiResult(new { ErrorMessage = ex.Message });
			}


		}
		[Route("ext/v1/query/events")]
		[HttpPost]
		public ActionResult Events(string appKey, string accessToken) {
			if (!VerifyToken(appKey, accessToken)) {
				return new ApiResult(new {
					Success = false,
					Message = "Invalid token"
				});
			}

			var eventTypes = Request.Query["eventTypes"].ToString();
			DateTime? since = new DateTime?();
			var sinceString = Request.Query["since"];
			if (!string.IsNullOrEmpty(sinceString)) {
				DateTime d;
				if (DateTime.TryParse(sinceString, out d)) {
					since = d;
				}
			}


			if (since.HasValue) {
				// Firstly go to the database and get everything after, since.
				using (var cn = DatabaseContext.Db.Events()) {
					var sqlEventTypes = string.Join(",", eventTypes.Split(',').Select(x => $"'{x.Replace("'", "''")}'"));
					var sql = $"SELECT * FROM mung.all WHERE at > @at AND event_type IN ({sqlEventTypes}) ORDER BY at ASC LIMIT 100";
					List<MungServerEvent> events = new List<MungServerEvent>();
					cn.SelectForEach(sql, new { at = since.Value }, (reader) => {
						events.Add(new MungServerEvent() {
							LogTime = ((DateTime)reader["at"]).ToUniversalTime(),
							Source = (string)reader["source"],
							Type = (string)reader["event_type"],
							AppId = (int)reader["app_id"],
							Data = JToken.Parse((string)reader["json_data"])
						});
					});

					if (events.Count > 0) {
						return new ApiResult(new { Events = events, Success = true, RealTime = false });
					}
				}
			}
			using (var request = MungApp.Current.LongPollProcessor.Listen(eventTypes.Split(','))) {

				var mungEvent = request.WaitForEvent(30 * 1000);
				if (mungEvent != null) {
					return new ApiResult(new { Events = new MungServerEvent[] { mungEvent }, Success = true, RealTime = true });
				}

				// Nothing to see here actually.
				return new ApiResult(new { Events = new MungServerEvent[] { }, Success = true, RealTime = false });
			}

		}
	}
}
