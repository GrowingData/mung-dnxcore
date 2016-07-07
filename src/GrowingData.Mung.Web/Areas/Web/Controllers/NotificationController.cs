using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using GrowingData.Mung;
using GrowingData.Utilities;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GrowingData.Mung.Web.Models;
using GrowingData.Mung.Core;
using System.IO;


namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class NotificationController : MungSecureController {
		// GET: Dashboards/CreateDashboard

		public NotificationController(IHostingEnvironment env) : base(env) {
		}

		[Route("notifications")]
		public ActionResult NotificationList(string url) {

			ViewBag.Notifications = Notification.List(CurrentUser.MungerId);
			return View("NotificationList");
		}

		private string BaseUrl {
			get {
				var type = Request.IsHttps ? "https" : "http";

				return $"{type}://{Request.Host}/";
			}
		}

	}
}