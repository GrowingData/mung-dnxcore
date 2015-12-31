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
		public int CreatedByMungUserId;
		public int UpdatedByMungUserId;

		public DateTime CreatedAt;
		public DateTime UpdatedAt;


		public static Dashboard Get(string url) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var dashboard = cn.ExecuteAnonymousSql<Dashboard>(
						@"SELECT * FROM Dashboard WHERE Url = @Url",
						 new { Url = url }
					)
					.FirstOrDefault();
				return dashboard;
			}
		}
		public static Dashboard Get(int dashboardId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var dashboard = cn.ExecuteAnonymousSql<Dashboard>(
						@"SELECT * FROM Dashboard WHERE DashboardId = @DashboardId",
						 new { DashboardId = dashboardId }
					)
					.FirstOrDefault();
				return dashboard;
			}
		}
		public static List<Dashboard> List(int MungUserId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var dashboards = cn.ExecuteAnonymousSql<Dashboard>(
						@"SELECT * FROM Dashboard",
						 null
					);
				return dashboards;
			}
		}
		public Dashboard Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var dbDashboard = cn.ExecuteAnonymousSql<Dashboard>(
					@"INSERT INTO Dashboard (Url, Title, CreatedAt, UpdatedAt, CreatedByMungUserId, UpdatedByMungUserId)
						SELECT @Url, @Title, @CreatedAt, @UpdatedAt, @CreatedByMungUserId, @UpdatedByMungUserId
					;

					SELECT * FROM Dashboard WHERE DashboardId = lastval();
					",
					this)
					.FirstOrDefault();
					

				return dbDashboard;
			}
		}


		public List<Graph> GetGraphs() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var components = cn.ExecuteAnonymousSql<Graph>(
					@"SELECT * FROM Graph WHERE DashboardId = @DashboardId",
					new { DashboardId = DashboardId }
				);
				return components;
			}

		}
	}
}