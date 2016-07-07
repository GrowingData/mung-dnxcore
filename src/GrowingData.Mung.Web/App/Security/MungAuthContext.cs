﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data.SqlClient;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {
	public static class MungAuthContext {



		public static void InitializeAuthentication(this HttpContext context) {
			
		}

		public static MungerIdentity CurrentMungerIdentity(this HttpContext context) {
			if (!context.Items.ContainsKey("munger")) {
				var identity = MungerIdentity.LoadIdentity(context);

				// Cache it for next time, even if its null (as the keys existence tells us not to 
				// try to load it next time.
				context.Items["munger"] = identity;
				return identity;
			}

			return context.Items["munger"] as MungerIdentity;
		}


		public static bool Login(this HttpContext context, Munger user) {
			if (user == null) {
				throw new ArgumentNullException("Unable to login a null user");
			}

			if (context == null) {
				throw new ArgumentNullException("Unable to login a user without a context");
			}

			var identity = new MungerIdentity(user, context);

			var signin = context.Authentication.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(identity)
			);


			signin.Wait();

			return true;
		}
		public static void Logout(this HttpContext context) {
			var signout = context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			
			signout.Wait();
		}

	}
}
