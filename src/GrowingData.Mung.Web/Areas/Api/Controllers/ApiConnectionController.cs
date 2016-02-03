using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.AspNet;
using GrowingData.Mung;
using GrowingData.Mung.Web.Models;
using Microsoft.AspNet.Hosting;
using GrowingData.Utilities;

using Newtonsoft.Json;

namespace GrowingData.Mung.Web.Areas.DashboardApi.Controllers {
	public class ApiConnectionController : MungSecureController {
		public ApiConnectionController(IHostingEnvironment env) : base(env) {
		}

		[HttpPost]
		[Route("api/connection/save")]
		public ActionResult Save(string name, int connectionId, string connectionString, int providerId) {

			Connection connection = null;
			if (connectionId > 0) {
				connection = Connection.Get(connectionId);
			}
			if (connection == null) {
				connection = new Connection() {
					CreatedByMunger = CurrentUser.MungerId,
					CreatedAt = DateTime.UtcNow,
				};
			}

			connection.Name = name;
			connection.ConnectionString = connectionString;
			connection.ProviderId = providerId;
			connection.UpdatedAt = DateTime.UtcNow;
			connection.UpdatedByMunger = CurrentUser.MungerId;

			connection.Save();

			MungApp.Current.ProcessInternalEvent("connection_create", new {
				dashboard_name = name,
				munger_id = CurrentUser.MungerId
			});
			return Json(new { Success = true, Connection = connection }, new JsonSerializerSettings() { Formatting = Formatting.Indented });

		}
		[HttpGet]
		[Route("api/connection/list")]
		public ActionResult List() {
			return Json(new { Connections = Connection.List(), Success = true }, new JsonSerializerSettings() { Formatting = Formatting.Indented });

		}
		[HttpGet]
		[Route("api/connection/{connectionName}/schema")]
		public ActionResult Schema(string connectionName) {
			var connection = Connection.Get(connectionName);
			if (connection == null) {
				return Json(new { Success = false, ErrorMessage = $"Unable to find connection with name: '{connectionName}'" });

			}
			var tables = connection.GetTables();
			var forSerialization = tables.Select(x => new {
				TableName = x.TableName,
				SchemaName = x.SchemaName,
				Columns = x.Columns.Select(c => new {
					ColumnName = c.ColumnName,
					Type = c.MungType.Code.ToString()
				})
			});


			return Json(new {
				Tables = forSerialization,
				Success = true
			}, new JsonSerializerSettings() { Formatting = Formatting.Indented });
		}

		[HttpPost]
		[Route("api/connection/test")]
		public ActionResult Test(string name, int connectionId, string connectionString, int providerId) {

			var connection = new Connection() {
				Name = name,
				ProviderId = providerId,
				ConnectionString = connectionString
			};

			try {
				var cn = connection.GetConnection();
				return Json(new { Success = true, Message = "Success" },
					new JsonSerializerSettings() { Formatting = Formatting.Indented }
				);
			} catch (Exception ex) {
				return Json(new { Success = false, Message = ex.Message },
					 new JsonSerializerSettings() { Formatting = Formatting.Indented }
				);
			}

		}
	}
}