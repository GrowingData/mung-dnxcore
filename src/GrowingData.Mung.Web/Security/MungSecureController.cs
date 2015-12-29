using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

using System.Security.Claims;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web.Models;
using Microsoft.AspNet.Mvc.Filters;

namespace GrowingData.Mung.Web {
	public class MungSecureController : Controller {
		protected readonly IHostingEnvironment _env;

		public MungSecureController(IHostingEnvironment env) {
			_env = env;

		}


		public override void OnActionExecuting(ActionExecutingContext context) {
			base.OnActionExecuting(context);
			ViewBag.HostingEnvironment = _env;
		}

		public Munger CurrentMunger {
			get {
				var id = CurrentMungerIdentity;
				if (id != null) {
					return id.Munger;
				}
				return null;
			}
		}
		public MungerIdentity CurrentMungerIdentity {
			get {

				return HttpContext.CurrentMungerIdentity();
			}
		}



	}
}
