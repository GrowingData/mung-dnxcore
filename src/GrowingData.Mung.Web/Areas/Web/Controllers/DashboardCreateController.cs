using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.DnxCore;


namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class DashboardCreateController : MungSecureController {

		public DashboardCreateController(IHostingEnvironment env) : base(env) {
		}
		[Route("create/dashboard/{*newUrl}")]
		public ActionResult Create(string newUrl, string title) {

			newUrl = "/" + newUrl;
			if (string.IsNullOrEmpty(title)) {
				title = newUrl
					.Replace("-", " ")
					.Replace("_", " ")
					.Replace("/", "");
			}

			using (var cn = DatabaseContext.Db.Mung()) {

				// Does the URL already exist?


				var existing = Dashboard.Get(newUrl);
				if (existing != null) {
					return Redirect($"/dashboard{newUrl}");
				}

				var dashboard = new Dashboard() {
					Url = newUrl,
					CreatedByMungerId = CurrentUser.MungerId,
					UpdatedByMungerId = CurrentUser.MungerId,
					Title = title,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow

				}.Insert();


				return Redirect($"/dashboard{dashboard.Url}");

			}
		}
	}
}