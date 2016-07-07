using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using GrowingData.Mung;
using GrowingData.Mung.Web.Models;
using Microsoft.AspNetCore.Hosting;
using GrowingData.Utilities;

using Newtonsoft.Json;

namespace GrowingData.Mung.Web.Areas.Api.Controllers {
	public class ApiDashboardController : MungSecureController {
		public ApiDashboardController(IHostingEnvironment env) : base(env) {
		}

		[HttpPost]
		[Route("api/dashboard/create")]
		public ActionResult Create(string name) {

			using (var cn = DatabaseContext.Db.Mung()) {

				// Does the URL already exist?


				var existing = Dashboard.Get(name);
				if (existing != null) {
					return Redirect($"/dashboard/{name}");
				}
				var dashboard = new Dashboard() {
					CreatedByMungerId = CurrentUser.MungerId,
					UpdatedByMungerId = CurrentUser.MungerId,
					Name = name,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow

				}.Insert();

				MungApp.Current.ProcessInternalEvent("dashboard_create", new {
					dashboard_name = name,
					munger_id = CurrentUser.MungerId
				});
				return Json(new { Success = true, Dashboard = dashboard });



			}
		}


		[HttpGet]
		[Route("api/dashboard/graphs")]
		public ActionResult ListGraphs(string url) {
			var dashboard = Dashboard.Get(url);
			if (dashboard == null) {
				throw new Exception("Unable to find the dashboard at the url: " + url);
			}

			var graphs = dashboard.GetGraphs();
			return Json(new {
				Graphs = graphs,
				Success = true
			}, new JsonSerializerSettings() { Formatting = Formatting.Indented });

		}
	}
}