using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.AspNet;
using GrowingData.Mung;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities;

using Newtonsoft.Json;

namespace GrowingData.Mung.Web.Areas.DashboardApi.Controllers {
	public class ApiFirehoseController : Controller {



		[HttpGet]
		[Route("api/firehose/poll")]
		public ActionResult SaveComponent(string eventTypes) {
			using (var request = MungApp.Current.Poller.Listen(eventTypes.Split(','))) {

				var mungEvent = request.WaitForEvent();
				return Json(new { Event = mungEvent, Success = true });
			}

		}

	}
}