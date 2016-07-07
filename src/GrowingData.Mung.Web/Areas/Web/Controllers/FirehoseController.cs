using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GrowingData.Mung;
using Microsoft.AspNetCore.Hosting;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class FirehoseController : MungSecureController {

		public FirehoseController(IHostingEnvironment env) : base(env) {
		}
		[Route("firehose")]
		public ActionResult FirehoseDefault() {
			ViewBag.Dashboards = Dashboard.List(CurrentUser.MungerId);



			return View("FirehoseDefault");
		}

	}
}