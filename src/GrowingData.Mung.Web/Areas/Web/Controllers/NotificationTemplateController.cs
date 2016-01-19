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
using HtmlAgilityPack;
using GrowingData.Utilities.DnxCore;
using Microsoft.AspNet.Mvc.Filters;

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
			var templatePath = WriteTemplate(notification.Name, notification.Template);
			
			ViewBag.ServerEvent = mse;

			var nameWithoutExtension = notification.Name.Split('.').FirstOrDefault();
			var result = View($"Email/{nameWithoutExtension}");

			return result;
		}

		public override void OnActionExecuted(ActionExecutedContext context) {
			base.OnActionExecuted(context);
		}

		[Route("notifications/preview")]
		[HttpPost]
		public ActionResult PreviewNotification() {

			var template = Request.Form["template"];

			// Try to load the event type from the document;
			var doc = new HtmlDocument();
			doc.LoadHtml(template);
			var eventType = doc.DocumentNode.SelectNodes("//mung-event-type").FirstOrDefault()?.InnerText;

			if (eventType == null) {
				return Json(new {
					Success = false,
					Message = "Please specify an event type, in the form \"<mung-event-type>YOUR_EVENT_TYPE</mung-event-type>\"",
					highlight = "mung-event-type"
				});
			}
			using (var cn = DatabaseContext.Db.Events()) {
				var sql = @"SELECT * FROM mung.all WHERE event_type = @eventType ORDER BY at DESC LIMIT 1";
				using (var reader = cn.DumpReader(sql, new { eventType = eventType })) {
					if (!reader.Read()) {
						return Json(new {
							Success = false,
							Message = $"The event type \"{eventType}\" does not yet exist.  Fire off a test event to enable live preview.",
							highlight = "mung-event-type"
						});
					}
					ViewBag.ServerEvent = new MungServerEvent() {
						LogTime = (DateTime)reader["at"],
						Source = (string)reader["source"],
						AppId = (int)reader["app_id"],
						Data = JToken.Parse((string)reader["json_data"])
					};
				}
			}

			ViewBag.Template = Request.Form["template"];


			var tmpName = Guid.NewGuid() + ".cshtml";

			var realPath = WriteTemplate(tmpName, template);

			var nameWithoutExtension = tmpName.Split('.').FirstOrDefault();

			ViewBag.TemplateRealPath = realPath;
			ViewBag.TemplatePartialPath = $"Email/{nameWithoutExtension}";

			return View("Preview");
		}

		public string WriteTemplate(string name, string templateCode) {

			// Write the template to somewhere awesome
			var appPath = new DirectoryInfo(_env.MapPath("")).Parent.FullName;


			var templatePath = Path.Combine(appPath, "Areas", "Web", "Views", "NotificationTemplate", "Email", $"{name}");
			PathHelpers.CreateDirectoryRecursively(templatePath);

			var templateFilePath = Path.Combine(templatePath, name);

			if (System.IO.File.Exists(templateFilePath)) {
				System.IO.File.Delete(templateFilePath);
			}

			System.IO.File.WriteAllText(templateFilePath, templateCode);

			return templatePath;
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
			WriteTemplate(notification.Name, notification.Template);

			ViewBag.ServerEvent = mse;

			var nameWithoutExtension = notification.Name.Split('.').FirstOrDefault();
			var result = View($"Email/{nameWithoutExtension}");



			return result;
		}

	}
}