using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Graph {
		public int GraphId;
		public int DashboardId;
		public int ConnectionId;
		public string Title;
		public string Html;
		public string Sql;
		public string Js;
		public double X;
		public double Y;
		public double Width;
		public double Height;

		public Graph Save(Dashboard dashboard) {
			// Make sure we are pointing to the right dashboard
			DashboardId = dashboard.DashboardId;

			if (GraphId <= 0) {
				return Insert(dashboard);
			} else {
				return Update(dashboard);
			}

		}

		public Graph Insert(Dashboard dashboard) {

			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					INSERT INTO graph(dashboard_id, connection_id, title, html, sql, js, x, y, width, height)
						VALUES (@DashboardId, @ConnectionId, @Title, @Html, @Sql, @Js, @X, @Y, @Width, @Height)
						RETURNING graph_id";

				GraphId = cn.DumpList<int>(sql, this).FirstOrDefault();

				return this;
			}
		}
		public Graph Update(Dashboard dashboard) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var graphs = dashboard.GetGraphs();
				if (graphs.Count(x => x.GraphId == this.GraphId) != 1) {
					throw new Exception("The graph specified does not belong to the dashboard with Url: " + dashboard.Url);
				}

				var sql = @"
					UPDATE Graph
						SET title = @Title, 
							connection_id = @ConnectionId,
							html = @Html, 
							sql = @Sql, 
							js = @Js, 
							x = @X, 
							y = @Y,
							width = @Width, 
							height = @Height
						WHERE graph_id = @GraphId";
				cn.ExecuteSql(sql, this);

				return this;
			}
		}

		/// <summary>
		/// Deletes a Graph from a Dashboard, checking that the Graph does infact
		/// live within the dashboard.
		/// </summary>
		/// <param name="dashboard"></param>
		/// <returns></returns>
		public bool Delete(Dashboard dashboard) {
			var graphs = dashboard.GetGraphs();

			using (var cn = DatabaseContext.Db.Mung()) {
				if (GraphId == -1) {
					throw new Exception("No GraphId was specified for deletion: " + dashboard.Url);
				} else {
					// Make sure that this graph actually belongs to this Dashboard
					if (graphs.Count(x => x.GraphId == GraphId) != 1) {
						throw new Exception("The graph specified does not belong to the dashboard with Url: " + dashboard.Url);
					}

					var sql = @"
						DELETE FROM Graph
							WHERE graph_id = @GraphId
							AND		dashboard_id = @DashboardId";

					cn.ExecuteSql(sql, new {
						ComponentId = GraphId,
						DashboardId = dashboard.DashboardId
					});

					return true;
				}
			}
		}

	}
}