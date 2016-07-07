﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data.SqlClient;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrowingData.Mung.Web {

	/// <summary>
	/// Any controller inheriting from this Controller must have 
	/// an Authenticated User, or else they will be sent to the login
	/// page.
	/// </summary>
	public class MungSecureController : Controller {
		protected readonly IHostingEnvironment _env;
		private Munger _currentMunger;

		public MungSecureController(IHostingEnvironment env) {
			_env = env;

		}


		public override void OnActionExecuting(ActionExecutingContext context) {
			base.OnActionExecuting(context);

			var identity = HttpContext.CurrentMungerIdentity();
			if (identity == null) {
				context.Result = Redirect($"/{Urls.LOGIN}");
                return;
			}

			_currentMunger = identity.User;
			ViewBag.HostingEnvironment = _env;

		}

		public Munger CurrentUser { get { return _currentMunger; } }
		public MungerIdentity CurrentUserIdentity { get { return HttpContext.CurrentMungerIdentity(); } }



	}
}
