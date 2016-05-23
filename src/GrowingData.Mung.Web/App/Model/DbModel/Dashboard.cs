using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GrowingData.Utilities.DnxCore;
using GrowingData.Utilities.Database;

namespace GrowingData.Mung.Web.Models {
	public class Dashboard : IBracketsEditable {
		public int DashboardId;
		public string Name;
		public string Css;
		public string Js;
		public int CreatedByMungerId;
		public int UpdatedByMungerId;

		public DateTime CreatedAt;
		public DateTime UpdatedAt;

		public string EncodedName { get { return WebUtility.UrlEncode(Name); } }

		public string ResourceUrl { get { return $"/{MungFileSystem.DashboardRootUrlPart}/{EncodedName}"; } }

		public static Dashboard Get(string name) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM dashboard WHERE name = @Name OR name = @DecodedName";
				var dashboard = cn.SelectAnonymous<Dashboard>(sql, new { Name = name, DecodedName = WebUtility.UrlDecode(name) })
					.FirstOrDefault();

				return dashboard;
			}
		}
		public static Dashboard Get(int dashboardId) {
			using (var cn = DatabaseContext.Db.Mung()) {

				var sql = @"SELECT * FROM dashboard WHERE dashboard_id = @DashboardId";
				var dashboard = cn.SelectAnonymous<Dashboard>(sql, new { DashboardId = dashboardId })
					.FirstOrDefault();

				return dashboard;
			}
		}
		public static List<Dashboard> List(int mungerId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM dashboard";
				var dashboards = cn.SelectAnonymous<Dashboard>(sql, null);
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
					INSERT INTO dashboard (name, created_by_munger, updated_by_munger)
						VALUES (@Name, @CreatedByMungerId, @UpdatedByMungerId)
						RETURNING dashboard_id
					";

				DashboardId = cn.SelectList<int>(sql, this).FirstOrDefault();
				return this;
			}
		}

		public Dashboard Update() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					UPDATE dashboard 
						SET 
							name = @Name, 
							updated_by_munger = @UpdatedByMungerId
						WHERE dashboard_id = @DashboardId
					";
				cn.ExecuteNonQuery(sql, null);

				return this;
			}
		}

		public void Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM dashboard WHERE dashboard_id = @DashboardId";
				cn.ExecuteNonQuery(sql, this);
			}
		}


		public List<Graph> GetGraphs() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					SELECT *, D.name AS DashboardName
					FROM graph G 
					INNER JOIN dashboard D
					ON G.dashboard_id = D.dashboard_id
					WHERE D.dashboard_id = @DashboardId";
				var components = cn.SelectAnonymous<Graph>(sql, this);
				return components;
			}
		}
	}
}