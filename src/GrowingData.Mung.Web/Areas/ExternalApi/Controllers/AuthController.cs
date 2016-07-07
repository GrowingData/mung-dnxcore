using System;
using System.Data.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;
using System.IO;
using System.Collections.Generic;
using JWT;

namespace GrowingData.Mung.Web.Areas.ExternalApi.Controllers {

	public class AuthController : ExternalApiController {


		[Route("ext/v1/auth/access_token")]
		[HttpGet]
		public ActionResult AccessToken() {

			var identity = HttpContext.CurrentMungerIdentity();
			if (identity != null) {
				var app = App.MungInternal;

				var payload = new Dictionary<string, object>() {
					{"clientTime", DateTime.UtcNow },
					{"name", identity.User.Name },
					{"email", identity.User.Email }
				};
				string token = JWT.JsonWebToken.Encode(payload, app.AppSecret, JwtHashAlgorithm.HS256);

				return new ApiResult(new { accessToken = token, success = true });

			}
			return new ApiResult(new { success = false, message = "Not logged in" });
		}
	}
}