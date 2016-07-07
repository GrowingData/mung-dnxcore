using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using GrowingData.Mung.Web;
using GrowingData.Mung.Web.Models;
using GrowingData.Utilities.DnxCore;

using Newtonsoft.Json;

namespace GrowingData.Mung.Web.Areas.Api.Controllers {
	public class ApiMungFileSystemController : MungSecureController {


		public ApiMungFileSystemController(IHostingEnvironment env) : base(env) {
		}



		[HttpGet]
		[Route("api/file-system/ls")]
		public ActionResult List() {

			var root = MungFileSystem.Hierarchy(CurrentUser);

			return Json(
				new { Success = true, Root = root },
				new JsonSerializerSettings() { Formatting = Formatting.Indented }
			);
		}



	}
}