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
	public class ApiMungFileSystemNotificationController : MungSecureController {
		public ApiMungFileSystemNotificationController(IHostingEnvironment env) : base(env) {
		}


		[HttpPost]
		[Route("api/file-system/notification/settings")]
		public ActionResult UpdateSettings(string fromAddress, string renderTemplateUrl, string accessKey, string accessSecret) {

			new Setting() { Key = SesNotificationSettings.SesFromAddressSettingKey, Value = fromAddress }.Save();
			new Setting() { Key = SesNotificationSettings.RenderTemplateUrlSettingKey, Value = renderTemplateUrl }.Save();
			new Setting() { Key = SesNotificationSettings.SesAccessKeySettingKey, Value = accessKey }.Save();
			new Setting() { Key = SesNotificationSettings.SesSecretKeySettingKey, Value = accessSecret }.Save();

			return Json(new { Success = true });

		}


		[HttpPost]
		[Route("api/file-system/notification/{name}/pause")]
		public ActionResult Pause(string name, bool paused) {
			var notification = Notification.Get(name);
			if (notification == null) {
				return new HttpNotFoundResult();
			}

			notification.IsPaused = paused;
			notification.Update();

			return Json(new {
				Success = true,
				Notification = notification,
				ResourceUrl = notification.ResourceUrl
			});
		}

		[HttpPost]
		[Route("api/file-system/notification/{name}/rename")]
		public ActionResult RenameGraph(string name) {

			var newPath = Request.Form["newPath"].ToString();
			var newPathParts = newPath.Split('/');

			var newNotificationName = newPathParts[2];

			var notification = Notification.Get(name);
			if (notification == null) {
				return new HttpNotFoundResult();
			}
			notification.Name = newNotificationName;
			notification.Update();

			return Json(new {
				Success = true,
				Message = "Success",
				IsNew = false,
				ResourceUrl = notification.ResourceUrl
			});
		}

		[HttpGet]
		[Route("api/file-system/notification/{name}")]
		public ActionResult Read(string name) {
			var notification = Notification.Get(name);
			if (notification == null) {
				return new HttpNotFoundResult();
			}

			return Content(notification.Template);
		}

		[HttpPut]
		[Route("api/file-system/notification/{name}")]
		public ActionResult Write(string name) {
			var notification = Notification.Get(name);
			var isNew = false;
			if (notification == null) {
				notification = new Notification() {
					Name = name,
					CreatedAt = DateTime.UtcNow,
					CreatedByMunger = CurrentUser.MungerId,
					IsPaused = false
				};
				isNew = true;

				MungApp.Current.ProcessInternalEvent("notification_create", new {
					notification_name = name,
					munger_id = CurrentUser.MungerId
				});
			}
			var content = Request.Form["data"].ToString();

			notification.Template = content;
			notification.UpdatedAt = DateTime.UtcNow;
			notification.UpdatedByMunger = CurrentUser.MungerId;
			notification.Save();

			MungApp.Current.ProcessInternalEvent("notification_save", new {
				notification_name = name,
				munger_id = CurrentUser.MungerId
			});

			return Json(new {
				Success = true,
				FileContent = content,
				IsNew = isNew,
				ResourceUrl = notification.ResourceUrl
			});
		}

		[HttpDelete]
		[Route("api/file-system/notification/{name}")]
		public ActionResult Delete(string name) {
			var notification = Notification.Get(name);
			notification.Delete();
			return Json(new { Success = true });
		}

	}
}