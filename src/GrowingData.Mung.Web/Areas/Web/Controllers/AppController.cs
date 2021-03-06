﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Auth.Controllers {
	[Area("Web")]
	public class AppController : MungSecureController {

		public AppController(IHostingEnvironment env) : base(env) {
		}



		[Route("apps")]
		[HttpGet]
		public ActionResult AppList() {
			var apps = App.List();

			ViewBag.Apps = apps;

			return View();
		}


	}
}