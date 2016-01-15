using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Auth.Controllers {
	[Area("Web")]
	public class MungController : MungSecureController {

		public MungController(IHostingEnvironment env) : base(env) {
		}





		[Route("mung/{*url}")]
		[HttpGet]
		public ActionResult Edit(string url) {
			return View();
		}


	}
}