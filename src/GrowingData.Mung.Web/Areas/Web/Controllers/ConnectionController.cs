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
	public class ConnectionController : MungSecureController {
		// GET: Dashboards/CreateDashboard

		public ConnectionController(IHostingEnvironment env) : base(env) {
		}

		[Route("connections")]
		public ActionResult ConnectionList(string url) {


			ViewBag.Connections = Connection.List();
			return View("ConnectionList");
		}
		
	}
}