using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung;
using GrowingData.Utilities;
using GrowingData.Mung.Web.Models;


namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Dashboard")]
	public class DashboardController : MungSecureController {
		// GET: Dashboards/CreateDashboard

		public DashboardController(IHostingEnvironment env) : base(env) {
		}

		[Route("dashboards")]
		public ActionResult ViewDashboardList(string url) {
			var munger = CurrentMunger;
			if (munger == null) {
				return Redirect("/login");
			}


			ViewBag.Dashboards = Dashboard.List(munger.MungerId);
			return View("DashboardList");
		}

		[Route("dashboard/{*url}")]
		public ActionResult ViewDashboard(string url) {

			var munger = CurrentMunger;
			if (munger == null) {
				return Redirect("/login");
			}

			url = "/" + url;
			using (var cn = DatabaseContext.Db.Metadata()) {

				// Does the URL already exist?

				var dashboard = Dashboard.Get(url);

				if (dashboard == null) {
					throw new Exception("Unable to find the dashboard at the url: " + url);
				}

				ViewBag.Dashboard = dashboard;
				ViewBag.Graphs = dashboard.GetGraphs();


			}
			return View("Dashboard");
		}
	}
}