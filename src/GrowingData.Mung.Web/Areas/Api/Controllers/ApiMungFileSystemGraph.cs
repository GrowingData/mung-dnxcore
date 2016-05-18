using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.DnxCore;

using Newtonsoft.Json;

namespace GrowingData.Mung.Web.Areas.Api.Controllers {
	public class ApiMungFileSystemGraphController : MungSecureController {

		public ApiMungFileSystemGraphController(IHostingEnvironment env) : base(env) {
		}


		[HttpGet]
		[Route("api/file-system/Dashboard/{dashboardName}/{graphName}")]
		public ActionResult Read(string dashboardName, string graphName) {
			var dashboard = Dashboard.Get(dashboardName);
			if (dashboard == null) {
				return new HttpNotFoundResult();
			}
			var graph = dashboard.GetGraphs().FirstOrDefault(g => g.Name == graphName);
			if (graph == null) {
				return new HttpNotFoundResult();
			}
			return Content(graph.Html);

		}

		[HttpPut]
		[Route("api/file-system/Dashboard/{dashboardName}/{graphName}")]
		public ActionResult Write(string dashboardName, string graphName) {
			var dashboard = Dashboard.Get(dashboardName);
			if (dashboard == null) {
				return new HttpNotFoundResult();
			}
			var graphs = dashboard.GetGraphs();
			var graph = graphs.FirstOrDefault(g => g.Name == graphName);

			var content = Request.Form["data"];
			var isNew = false;
			if (graph == null) {

				var maxHeight = graphs.Count == 0 ? 0 : graphs.Select(g => g.Y + g.Height).Max();

				graph = new Graph() {
					DashboardId = dashboard.DashboardId,
					DashboardName = dashboard.Name,
					Name = graphName,
					X = 0,
					Y = maxHeight,
					Height = 4,
					Width = 12
				};
				isNew = true;

				MungApp.Current.ProcessInternalEvent("graph_create", new {
					dashboard_id = dashboard.DashboardId,
					graph_name = graphName,
					munger_id = CurrentUser.MungerId
				});
			}
			graph.Html = content;
			graph.Save(dashboard);

			MungApp.Current.ProcessInternalEvent("graph_save", new {
				dashboard_id = dashboard.DashboardId,
				graph_name = graphName,
				munger_id = CurrentUser.MungerId
			});

			return Json(new { Success = true, FileContent = content, IsNew = isNew, ResourceUrl = graph.ResourceUrl });
		}

		[HttpDelete]
		[Route("api/file-system/Dashboard/{dashboardName}/{graphName}")]
		public ActionResult Delete(string dashboardName, string graphName) {
			var dashboard = Dashboard.Get(dashboardName);
			if (dashboard == null) {
				return new HttpNotFoundResult();
			}
			var graph = dashboard.GetGraphs().FirstOrDefault(g => g.Name == graphName);
			if (graph == null) {
				return new HttpNotFoundResult();
			}
			graph.Delete(dashboard);

			MungApp.Current.ProcessInternalEvent("graph_delete", new {
				dashboard_id = dashboard.DashboardId,
				graph_name = graphName,
				munger_id = CurrentUser.MungerId
			});
			return Json(new { Success = true });
		}

		[HttpPost]
		[Route("api/file-system/Dashboard/{dashboardName}/{graphName}/size")]
		public ActionResult SaveGraphSize(string dashboardName, string graphName, float x, float y, float height, float width) {
			var dashboard = Dashboard.Get(dashboardName);
			if (dashboard == null) {
				return new HttpNotFoundResult();
			}
			var graph = dashboard.GetGraphs().FirstOrDefault(g => g.Name == graphName);
			if (graph == null) {
				return new HttpNotFoundResult();
			}

			graph.X = x;
			graph.Y = y;
			graph.Height = height;
			graph.Width = width;

			graph.Update(dashboard);
			return Json(new { Success = true, Message = "Success", IsNew = false });

		}

		[HttpPost]
		[Route("api/file-system/Dashboard/{dashboardName}/{graphName}/rename")]
		public ActionResult RenameGraph(string dashboardName, string graphName) {
			var dashboard = Dashboard.Get(dashboardName);
			if (dashboard == null) {
				return new HttpNotFoundResult();
			}
			var graph = dashboard.GetGraphs().FirstOrDefault(g => g.Name == graphName);
			if (graph == null) {
				return new HttpNotFoundResult();
			}

			var newPath = Request.Form["newPath"].ToString();
			var newPathParts = newPath.Split('/');

			var newDashboardName = newPathParts[2];
			var newGraphName = newPathParts[3];

			var newDashboard = Dashboard.Get(newDashboardName);
			if (newDashboard == null) {
				return new HttpNotFoundResult();
			}

			graph.DashboardId = newDashboard.DashboardId;
			graph.Name = newGraphName;

			graph.Save(newDashboard);
			return Json(new { Success = true, Message = "Success", IsNew = false });

		}
	}
}