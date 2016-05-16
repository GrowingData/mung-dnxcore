using System;
using System.Data.Common;
using System.IO;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;

using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web.Models;
using Microsoft.AspNet.Cors;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Mvc.Filters;

namespace GrowingData.Mung.Web {

	public class ExternalApiController : Controller {


		public override void OnActionExecuting(ActionExecutingContext context) {
			base.OnActionExecuting(context);

		}


		public bool VerifyToken(string appKey, string token) {
			JWT.JsonWebToken.JsonSerializer = JwtHelper.Serializer;

			var app = App.Get(appKey);
			if (app == null) {
				return false;
			}

			//using (var reader = new StreamReader(Request.Body)) {
			//	var tokenString = reader.ReadToEnd();

			try {
				string jsonPayload = JWT.JsonWebToken.Decode(token, app.AppSecret);


				return true;

			} catch (JWT.SignatureVerificationException) {
				return false;
				Console.WriteLine("Invalid token!");
			}

			//}
		}

		public T VerifyToken<T>(string appKey, string token) where T : class {
			JWT.JsonWebToken.JsonSerializer = JwtHelper.Serializer;

			var app = App.Get(appKey);

			//using (var reader = new StreamReader(Request.Body)) {
			//	var tokenString = reader.ReadToEnd();

			try {
				string jsonPayload = JWT.JsonWebToken.Decode(token, app.AppSecret);

				var obj = JToken.Parse(jsonPayload);
				return obj.ToObject<T>();


			} catch (JWT.SignatureVerificationException) {
				return null;
				Console.WriteLine("Invalid token!");
			}

			//}


		}

	}
}
