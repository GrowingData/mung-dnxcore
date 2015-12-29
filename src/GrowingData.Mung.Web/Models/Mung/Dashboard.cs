using System;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Mung;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Dashboard {
		public int DashboardId;
		public string Url;
		public string Title;
		public string Css;
		public string Js;
		public int CreatedByUserId;
		public int ModifiedByUserId;


		public static Dashboard Get(string url) {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var dashboard = cn.ExecuteAnonymousSql<Dashboard>(
						@"SELECT * FROM mung.Dashboard WHERE Url = @Url",
						 new { Url = url }
					)
					.FirstOrDefault();
				return dashboard;
			}
		}
		public static Dashboard Get(int dashboardId) {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var dashboard = cn.ExecuteAnonymousSql<Dashboard>(
						@"SELECT * FROM mung.Dashboard WHERE DashboardId = @DashboardId",
						 new { DashboardId = dashboardId }
					)
					.FirstOrDefault();
				return dashboard;
			}
		}
		public static List<Dashboard> List(int mungerId) {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var dashboards = cn.ExecuteAnonymousSql<Dashboard>(
						@"SELECT * FROM mung.Dashboard",
						 null
					);
				return dashboards;
			}
		}


		public List<Graph> GetGraphs() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var components = cn.ExecuteAnonymousSql<Graph>(
					@"SELECT * FROM mung.Graph WHERE DashboardId = @DashboardId",
					new { DashboardId = DashboardId }
				);
				return components;
			}

		}
	}
}