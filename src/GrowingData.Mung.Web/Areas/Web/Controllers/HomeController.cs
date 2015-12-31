using System.Linq;
using System;
using Microsoft.AspNet.Mvc;
using GrowingData.Mung;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class HomeController : MungSecureController {
		// GET: Dashboards/CreateDashboard

		public HomeController(IHostingEnvironment env) : base(env) {
		}

		[Route("")]
		public ActionResult Default() {
			Console.WriteLine("HomeController.Default");
			var munger = CurrentUser;
			if (munger == null) {
				return Redirect("/" + Urls.LOGIN);
			}

			ViewBag.Dashboards = Dashboard.List(CurrentUser.MungUserId);



			return View("Default");
		}

	}
}