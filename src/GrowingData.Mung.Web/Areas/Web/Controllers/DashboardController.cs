using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung;
using GrowingData.Utilities;
using GrowingData.Mung.Web.Models;


namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class DashboardController : MungSecureController {
		// GET: Dashboards/CreateDashboard

		public DashboardController(IHostingEnvironment env) : base(env) {
		}

		[Route("dashboards")]
		public ActionResult ViewDashboardList(string url) {
			ViewBag.Dashboards = Dashboard.List(CurrentUser.MungerId);
			return View("DashboardList");
		}

		[Route("dashboard/{name}")]
		public ActionResult ViewDashboard(string name) {
			// Does the URL already exist?

			var dashboard = Dashboard.Get(name);

			if (dashboard == null) {
				return new HttpNotFoundResult();
			}

			ViewBag.Dashboard = dashboard;
			ViewBag.Graphs = dashboard.GetGraphs();


			return View("Dashboard");
		}
	}
}