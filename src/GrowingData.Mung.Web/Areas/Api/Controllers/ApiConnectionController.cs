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

namespace GrowingData.Mung.Web.Areas.Api.Controllers {
	public class ApiConnectionController : MungSecureController {
		public ApiConnectionController(IHostingEnvironment env) : base(env) {
		}

		[HttpPost]
		[Route("api/connection/Save")]
		public ActionResult Save(int connectionId, string name, string connectionString, int providerId) {

			var connection = Connection.Get(connectionId);

			if (connection == null) {
				connection = new Connection() {
					Name = name,
					ProviderId = providerId,
					ConnectionString = connectionString,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
					CreatedByMunger = this.CurrentUser.MungerId,
					UpdatedByMunger = this.CurrentUser.MungerId
				};
				connection.Insert();
			} else {
				connection.Name = name;
				connection.ProviderId = providerId;
				connection.ConnectionString = connectionString;
				connection.UpdatedByMunger = this.CurrentUser.MungerId;
				connection.UpdatedAt = DateTime.UtcNow;
				connection.Update();
			}


			return Json(new {
				Success = true,
				Connection = connection
			}, new JsonSerializerSettings() {
				Formatting = Formatting.Indented
			});

		}


		[HttpPost]
		[Route("api/connection/test")]
		public ActionResult TestConnection(string name, int connectionId, string connectionString, int providerId) {
			// Create a new connection and try to connect to it

			var cn = new Connection() {
				ConnectionString = connectionString,
				Name = name,
				ProviderId = providerId
			};
			try {
				using (var sqlcn = cn.GetConnection()) {
					return Json(new {
						Success = true,
						Message = "Success"
					}, new JsonSerializerSettings() {
						Formatting = Formatting.Indented
					});
				}
			} catch (Exception ex) {
				return Json(new {
					Success = false,
					Message = ex.Message
				}, new JsonSerializerSettings() {
					Formatting = Formatting.Indented
				});

			}
		}
	}
}