﻿using System;
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
	public class ApiAppController : MungSecureController {
		public ApiAppController(IHostingEnvironment env) : base(env) {
		}

		[Route("api/app/create")]
		[HttpPost]
		public ActionResult Create(string name) {

			if (CurrentUser == null || !CurrentUser.IsAdmin) {
				return Redirect(Urls.LOGIN);
			}

			var app = new App(name, CurrentUser.MungerId);

			app.Insert();

			return Json(new { Message = "Success", Success = true });
		}

	}
}