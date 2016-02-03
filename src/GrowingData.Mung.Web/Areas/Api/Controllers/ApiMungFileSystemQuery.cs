using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.DnxCore;

using Newtonsoft.Json;

namespace GrowingData.Mung.Web.Areas.DashboardApi.Controllers {
	public class ApiMungFileSystemQueryController : MungSecureController {
		public ApiMungFileSystemQueryController(IHostingEnvironment env) : base(env) {
		}


		[HttpPost]
		[Route("api/file-system/query/{name}/rename")]
		public ActionResult RenameGraph(string name) {

			var newPath = Request.Form["newPath"].ToString();
			var newPathParts = newPath.Split('/');

			var newQueryName = newPathParts[2];

			var query = Query.Get(name);
			if (query == null) {
				return new HttpNotFoundResult();
			}
			query.Name = newQueryName;
			query.Update();

			return Json(new {
				Success = true,
				Message = "Success",
				IsNew = false,
				ResourceUrl = query.ResourceUrl
			});
		}

		[HttpGet]
		[Route("api/file-system/query/{name}")]
		public ActionResult Read(string name) {
			var query = Query.Get(name);
			if (query == null) {
				return new HttpNotFoundResult();
			}

			return Content(query.Code);
		}

		[HttpPut]
		[Route("api/file-system/query/{name}")]
		public ActionResult Write(string name) {
			var query = Query.Get(name);
			var isNew = false;
			if (query == null) {
				query = new Query() {
					Name = name,
					Path = "",
					CreatedAt = DateTime.UtcNow,
					CreatedByMunger = CurrentUser.MungerId
				};
				isNew = true;

				MungApp.Current.ProcessInternalEvent("query_create", new {
					query_name = name,
					munger_id = CurrentUser.MungerId
				});
			}
			var content = Request.Form["data"].ToString();

			query.Code = content;
			query.UpdatedAt = DateTime.UtcNow;
			query.UpdatedByMunger = CurrentUser.MungerId;
			query.Save();

			MungApp.Current.ProcessInternalEvent("query_save", new {
				query_name = name,
				munger_id = CurrentUser.MungerId
			});

			return Json(new {
				Success = true,
				FileContent = content,
				IsNew = isNew,
				ResourceUrl = query.ResourceUrl
			});
		}

		[HttpDelete]
		[Route("api/file-system/query/{name}")]
		public ActionResult Delete(string name) {
			var query = Query.Get(name);
			query.Delete();
			return Json(new { Success = true });
		}

	}
}