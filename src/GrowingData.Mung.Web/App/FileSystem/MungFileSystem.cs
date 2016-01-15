using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {
	public class MungFileSystem {
		public const string DashboardRootUrlPart = "Dashboard";
		public const string NotificationRootUrlPart = "Notification";

		public static MungFileSystemEntry Hierarchy(Munger currentUser) {


			var root = new MungFileSystemEntry("directory", "root", "MUNG", "/", null);

			var dashboards = new MungFileSystemEntry("directory", "dashboard-root", DashboardRootUrlPart, $"/{DashboardRootUrlPart}/", root);
            var allGraphs = Graph.GetGraphs();

			foreach (var d in Dashboard.List(currentUser.MungerId)) {
				var dfs = new MungFileSystemEntry("directory", "dashboard", d.Name, d.ResourceUrl, dashboards);

				if (allGraphs.ContainsKey(d.DashboardId)) {
					var graphs = allGraphs[d.DashboardId];
					foreach (var g in graphs) {
						var gfs = new MungFileSystemEntry("file", "graph", g.Name, g.ResourceUrl, dfs);
					}
				}
			}

			var notifications = new MungFileSystemEntry("directory", "notification-root", NotificationRootUrlPart, $"/{NotificationRootUrlPart}", root);

			foreach (var n in Notification.List(currentUser.MungerId)) {
				var dfs = new MungFileSystemEntry("file", "notification", n.Name, n.ResourceUrl, notifications);
			}

			return root;

		}

	}
}
