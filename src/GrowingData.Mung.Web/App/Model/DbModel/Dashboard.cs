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
		public int CreatedByMungerId;
		public int UpdatedByMungerId;

		public DateTime CreatedAt;
		public DateTime UpdatedAt;


		public static Dashboard Get(string url) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM dashboard WHERE url = @Url";
				var dashboard = cn.ExecuteAnonymousSql<Dashboard>(sql, new { Url = url })
					.FirstOrDefault();

				return dashboard;
			}
		}
		public static Dashboard Get(int dashboardId) {
			using (var cn = DatabaseContext.Db.Mung()) {

				var sql = @"SELECT * FROM dashboard WHERE dashboard_id = @DashboardId";
				var dashboard = cn.ExecuteAnonymousSql<Dashboard>(sql, new { DashboardId = dashboardId })
					.FirstOrDefault();

				return dashboard;
			}
		}
		public static List<Dashboard> List(int mungerId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM dashboard";
				var dashboards = cn.ExecuteAnonymousSql<Dashboard>(sql, null);
				return dashboards;
			}
		}

		public Dashboard Save() {
			if (DashboardId <= 0) {
				return Insert();
			} else {
				return Update();
			}
		}

		public Dashboard Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					INSERT INTO dashboard (url, title, created_by_munger, updated_by_munger)
						VALUES (@Url, @Title, @CreatedByMungerId, @UpdatedByMungerId)
						RETURNING dashboard_id
					";

				DashboardId = cn.DumpList<int>(sql, this).FirstOrDefault();
				return this;
			}
		}

		public Dashboard Update() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					UPDATE dashboard 
						SET 
							url=@Url, 
							title = Title, 
							updated_by_munger = @UpdatedByMungerId
						WHERE dashboard_id = @DashboardId
					";
				cn.ExecuteSql(sql, null);

				return this;
			}
		}

		public void Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM dashboard WHERE dashboard_id = @DashboardId";
				cn.ExecuteSql(sql, this);
			}
		}


		public List<Graph> GetGraphs() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM graph WHERE dashboard_id = @DashboardId";
				var components = cn.ExecuteAnonymousSql<Graph>(sql, this);
				return components;
			}

		}
	}
}