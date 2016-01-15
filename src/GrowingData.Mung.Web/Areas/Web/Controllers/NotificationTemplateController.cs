using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Rendering;
using GrowingData.Mung;
using GrowingData.Utilities;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GrowingData.Mung.Web.Models;
using GrowingData.Mung.Core;
using System.IO;


namespace GrowingData.Mung.Web.Areas.Dashboards.Controllers {

	[Area("Web")]
	public class NotificationTemplateController : Controller {
		// GET: Dashboards/CreateDashboard
		IHostingEnvironment _env;

		public NotificationTemplateController(IHostingEnvironment env) {
			_env = env;
		}

		[Route("notifications/generate")]
		[HttpPost]
		public ActionResult GenerateNotification() {

			var notificationId = int.Parse(Request.Form["notificationId"]);
			var notification = Notification.Get(notificationId);

			if (notification == null) {
				throw new Exception($"Unable to load the notification with notificationId={notificationId}");
			}
			

			var mse = JsonConvert.DeserializeObject<MungServerEvent>(Request.Form["event"]);

			// Write the template to somewhere awesome
			var appPath = new DirectoryInfo(_env.MapPath("")).Parent.FullName;
			var templatePath = Path.Combine(appPath, "Areas", "Web", "Views", "NotificationTemplate", "Email", $"{notification.Name}");

			if (!System.IO.File.Exists(templatePath)) {
				System.IO.File.Delete(templatePath);
			}
			System.IO.File.WriteAllText(templatePath, notification.Template);
			ViewBag.ServerEvent = mse;

			var nameWithoutExtension = notification.Name.Split('.').FirstOrDefault();
			var result = View($"Email/{nameWithoutExtension}");
			return result;
		}


		[Route("notifications/generate/test")]
		[HttpGet]
		public ActionResult GenerateTestNotification(int notificationId) {

			//var notificationId = int.Parse(Request.Form["notificationId"]);
			var notification = Notification.Get(notificationId);

			if (notification == null) {
				throw new Exception($"Unable to load the notification with notificationId={notificationId}");
			}



			var mse = new MungServerEvent() {
				LogTime = DateTime.UtcNow,
				Source = Dns.GetHostName(),
				AppId = -1,
				Data = JToken.Parse(
					JsonConvert.SerializeObject(new {
						User = new {
							Username = "john",
							FirstName = "John",
							Email = "john@johniejohnjohn.com"
						},
						PasswordResetToken = "shJwFOfIQjy6c3dkjf8id3"
					})
				)
			};



			//var mse = JsonConvert.DeserializeObject<MungServerEvent>(Request.Form["event"]);

			// Write the template to somewhere awesome
			var appPath = new DirectoryInfo(_env.MapPath("")).Parent.FullName;
			var templatePath = Path.Combine(appPath, "Areas", "Web", "Views", "NotificationTemplate", "Email", $"{notification.Name}");

			if (!System.IO.File.Exists(templatePath)) {
				System.IO.File.Delete(templatePath);
			}
			System.IO.File.WriteAllText(templatePath, notification.Template);
			ViewBag.ServerEvent = mse;

			var nameWithoutExtension = notification.Name.Split('.').FirstOrDefault();
			var result = View($"Email/{nameWithoutExtension}");



			return result;
		}

	}
}