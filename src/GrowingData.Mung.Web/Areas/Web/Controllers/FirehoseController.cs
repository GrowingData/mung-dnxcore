using System.Linq;
using Microsoft.AspNet.Mvc;
using GrowingData.Mung;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class FirehoseController : MungSecureController {

		public FirehoseController(IHostingEnvironment env) : base(env) {
		}
		[Route("firehose")]
		public ActionResult FirehoseDefault() {
			var munger = CurrentUser;
			if (munger == null) {
				return Redirect("/login");
			}


			ViewBag.Dashboards = Dashboard.List(munger.MungUserId);



			return View("FirehoseDefault");
		}

	}
}