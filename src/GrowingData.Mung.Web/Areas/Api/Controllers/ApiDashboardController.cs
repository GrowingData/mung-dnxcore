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
	public class ApiDashboardController : Controller {


		[HttpPost]
		[Route("api/dashboard/graph")]
		public ActionResult SaveComponent(string url, string graphJson) {
			var dashboard = Dashboard.Get(url);
			if (dashboard == null) {
				throw new Exception("Unable to find the dashboard at the url: " + url);
			}

			var toSave = JsonConvert.DeserializeObject<Graph>(graphJson);

			toSave.Save(dashboard);

			return Json(new { Success = true, Message = "Success" });
		}

		[HttpDelete]
		[Route("api/dashboard/graph")]
		public ActionResult DeleteComponent(string url, string graphJson) {
			url = "/" + url;
			var dashboard = Dashboard.Get(url);
			if (dashboard == null) {
				throw new Exception("Unable to find the dashboard at the url: " + url);
			}

			var graphs = dashboard.GetGraphs();
			var toDelete = JsonConvert.DeserializeObject<Graph>(graphJson);

			if (toDelete.Delete(dashboard)) {
				return Json(new { Success = true, Message = "Success" });
			} else {
				return Json(new { Success = false, Message = "Unable to save Graph" });
			}

		}
	}
}