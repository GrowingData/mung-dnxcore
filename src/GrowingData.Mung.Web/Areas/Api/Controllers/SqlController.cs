using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;

using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.ApiData.Controllers {
	public class MungSqlController : MungSecureController {


		public MungSqlController(IHostingEnvironment env) : base(env) {
		}

		public DbConnection GetConnection(int? connectionId) {
			if (connectionId.HasValue && connectionId.Value != -1) {
				var connection = Connection.Get(connectionId.Value);
				return connection.GetConnection();
			} else {
				return DatabaseContext.Db.Warehouse();
			}
		}


		[Route("api/sql/mung")]
		[HttpPost]
		public ActionResult Index(string sql, int? connectionId) {


			Response.ContentType = "application/json";
			try {
				using (var cn = GetConnection(connectionId)) {
					return Content(cn.DumpJsonRows(sql, null));
				}
			} catch (Exception ex) {

				return Json(new { ErrorMessage = ex.Message });

			}


		}
	}
}