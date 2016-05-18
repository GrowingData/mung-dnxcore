using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Api.Controllers {
	public class ApiQueryController : MungSecureController {
		public ApiQueryController(IHostingEnvironment env) : base(env) {
		}

		public DbConnection GetConnection(int? connectionId) {
			if (connectionId.HasValue && connectionId.Value != -1) {
				var connection = Connection.Get(connectionId.Value);
				return connection.GetConnection();
			} else {
				return DatabaseContext.Db.Events();
			}
		}

		[Route("api/query-chunk")]
		public ActionResult Query() {
			Response.ContentType = "text/html";
			//Response.Headers["Transfer-Encoding"] = "chunked";
			for (var i = 0; i < 30; i++) {

				using (var writer = new StreamWriter(Response.Body)) {
					writer.WriteLine("Hello " + i);
					System.Threading.Thread.Sleep(1000);
				}
			}
			return new EmptyResult();
		}

		[Route("api/query")]
		[HttpPost]
		public ActionResult Query(string sql, string format) {

			var parameters = Request.Query.ToDictionary(q => q.Key, q => q.Value as object);

			if (format == "json") {
				WriteJson(sql, parameters);
			}

			if (format.StartsWith("jsonp_row")) {
				WriteRowsScript(sql, parameters, format);
			}

			if (format.StartsWith("jsonp_count")) {
				WriteCount(sql, parameters, format);
			}

			return new EmptyResult();
		}

		public void WriteCount(string sql, Dictionary<string, object> parameters, string format) {
			Response.ContentType = "text/html";
			using (var writer = new StreamWriter(Response.Body)) {
				var mod = int.Parse(format.Split('_').Last());

				writer.Write("<html><body>");

				int rowCount = 0;
				try {
					var query = MungQuery.Execute(sql, parameters, (row) => {
						rowCount++;
						if (rowCount % mod == 0) {
							writer.Write($"<script type=\"text/javascript\">parent.MUNG.Callbacks.get(\"{format}\").row({rowCount});</script>\n");
							Response.Body.Flush();

						}
					});
				} catch (Exception ex) {
					WriteStreamException(sql, format, ex, writer);
					return;
				}

				writer.Write($"<script type=\"text/javascript\">parent.MUNG.Callbacks.get(\"{format}\").complete();</script>\n");
				writer.Write("</body></html>");
			}
		}

		public void WriteRowsScript(string sql, Dictionary<string, object> parameters, string format) {

			Response.ContentType = "text/html";
			using (var writer = new StreamWriter(Response.Body)) {
				using (var jsonWriter = new JsonTextWriter(writer)) {
					writer.Write("<html><body>");
					try {
						var query = MungQuery.Execute(sql, parameters, (row) => {

							writer.Write($"<script type=\"text/javascript\">parent.MUNG.Callbacks.get(\"{format}\").row(");
							jsonWriter.WriteStartObject();
							for (var i = 0; i < row.FieldCount; i++) {
								jsonWriter.WritePropertyName(row.GetName(i));
								var val = row[i];
								if (val == DBNull.Value) {

									jsonWriter.WriteNull();
								} else {
									jsonWriter.WriteValue(val);
								}
							}
							jsonWriter.WriteEndObject();
							writer.Write(");</script>\n");
							Response.Body.Flush();
						});
					} catch (Exception ex) {
						WriteStreamException(sql, format, ex, writer);
						return;
					}
					writer.Write($"<script type=\"text/javascript\">parent.MUNG.Callbacks.get(\"{format}\").complete();</script>\n");
					writer.Write("</body></html>");
				}
			}
		}

		public void WriteStreamException(string sql, string format, Exception ex, StreamWriter writer) {
			var errorObj = new {
				Message = ex.Message,
				Sql = sql
			};
			var errorJson = JsonConvert.SerializeObject(errorObj);

			writer.Write($"<script type=\"text/javascript\">parent.MUNG.Callbacks.get(\"{format}\").error({errorJson});</script>\n");
			Response.Body.Flush();
			writer.Write("</body></html>");
			writer.Write("</body></html>");
			return;
		}

		public void WriteJson(string sql, Dictionary<string, object> parameters) {

			Response.ContentType = "application/json";
			int rowNumber = 0;

			using (var writer = new StreamWriter(Response.Body)) {
				using (var jsonWriter = new JsonTextWriter(writer)) {
					jsonWriter.WriteStartObject();

					jsonWriter.WritePropertyName("Success");
					jsonWriter.WriteValue(true);


					var query = MungQuery.Execute(sql, parameters, (row) => {
						if (rowNumber == 0) {

							jsonWriter.WritePropertyName("ColumnNames");
							jsonWriter.WriteStartArray();

							for (var i = 0; i < row.FieldCount; i++) {
								jsonWriter.WriteValue(row.GetName(i));
							}
							jsonWriter.WriteEndArray();

							jsonWriter.WritePropertyName("Rows");
							jsonWriter.WriteStartArray();
						}

						rowNumber++;

						jsonWriter.WriteStartObject();

						for (var i = 0; i < row.FieldCount; i++) {
							jsonWriter.WritePropertyName(row.GetName(i));
							jsonWriter.WriteValue(row[i]);
						}

						jsonWriter.WriteEndObject();
						Response.Body.Flush();
					});

					jsonWriter.WriteEndArray();
					jsonWriter.WriteEndObject();
				}
			}
		}
	}
}