using System;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using System.IO;

using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Cors;

namespace GrowingData.Mung.Web {
	public class ApiResult : ActionResult {

		private object _result;

		public ApiResult(object obj) {
			_result = obj;
		}
		
		public override void ExecuteResult(ActionContext context) {

			var req = context.HttpContext.Request;
			var resp = context.HttpContext.Response;
			
			resp.AllowCors();

			var fmt = req.Query["format"];
			var callback = req.Query["callback"];

			if (!string.IsNullOrEmpty(fmt)) {

				if (fmt == "json") {
					WriteJson(context);
					return;
				}
				if (fmt == "jsonp") {
					if (string.IsNullOrEmpty(callback)) {
						throw new Exception("Unable to write JSONP, a query string parameter named 'callback' was not found");
					}
					WriteJsonP(context, callback);
					return;
				}



			}

			// Nothing specified, so guess it.
			if (!string.IsNullOrEmpty(callback)) {
				WriteJsonP(context, callback);
				return;
			}


			// Default, just use JSON
			WriteJson(context);
		}

		private void WriteJsonP(ActionContext context, string callback) {

			var req = context.HttpContext.Request;
			var resp = context.HttpContext.Response;
			// We are doing JSONP Yo
			var json = JsonConvert.SerializeObject(_result);
			resp.ContentType = "application/x-javascript";
			resp.Write(string.Format("{0}({1});", callback, json));

			return;
		}

		private void WriteJson(ActionContext context) {

			var req = context.HttpContext.Request;
			var resp = context.HttpContext.Response;

			var json = JsonConvert.SerializeObject(_result);

			resp.ContentType = "application/json; charset=utf-8";
			resp.Write(json);
		}
	}
}
