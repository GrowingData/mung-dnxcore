using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Utilities.DnxCore;
using System.Net;

namespace GrowingData.Mung.Web.Models {
	public class Graph {
		public int GraphId;
		public int DashboardId;
		public string Name;
		public string DashboardName;
		public string Html;
		public double X;
		public double Y;
		public double Width;
		public double Height;

		public string ResourceUrl { get { return $"/{MungFileSystem.DashboardRootUrlPart}/{WebUtility.UrlEncode(DashboardName)}/{Name}"; } }

		public Graph Save(Dashboard dashboard) {
			// Make sure we are pointing to the right dashboard
			DashboardId = dashboard.DashboardId;

			if (GraphId <= 0) {
				return Insert(dashboard);
			} else {
				return Update(dashboard);
			}

		}

		public static Dictionary<int, List<Graph>> GetGraphs() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					SELECT G.*, D.name AS DashboardName
					FROM graph G 
					INNER JOIN dashboard D
					ON G.dashboard_id = D.dashboard_id";
				var graphs = cn.ExecuteAnonymousSql<Graph>(sql, null);

				var grouped = graphs.GroupBy(g => g.DashboardId);

				var dashboards = new Dictionary<int, List<Graph>>();
				foreach(var group in grouped) {
					dashboards[group.Key] = group.ToList();
				}
				return dashboards;
			}
		}


		public Graph Insert(Dashboard dashboard) {

			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					INSERT INTO graph(dashboard_id, name, html, x, y, width, height)
						VALUES (@DashboardId, @Name, @Html, @X, @Y, @Width, @Height)
						RETURNING graph_id";

				GraphId = cn.DumpList<int>(sql, this).FirstOrDefault();

				return this;
			}
		}
		public Graph Update(Dashboard dashboard) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var graphs = dashboard.GetGraphs();
				if (graphs.Count(x => x.GraphId == this.GraphId) != 1) {
					throw new Exception("The graph specified does not belong to the dashboard with Name: " + dashboard.Name);
				}

				var sql = @"
					UPDATE Graph
						SET name = @Name, 
							html = @Html, 
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
					throw new Exception("No GraphId was specified for deletion: " + dashboard.Name);
				} else {
					// Make sure that this graph actually belongs to this Dashboard
					if (graphs.Count(x => x.GraphId == GraphId) != 1) {
						throw new Exception("The graph specified does not belong to the dashboard with Name: " + dashboard.Name);
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