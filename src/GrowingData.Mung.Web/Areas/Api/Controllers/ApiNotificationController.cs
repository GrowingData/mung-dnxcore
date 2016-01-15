using System;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;

namespace GrowingData.Mung.Web.Areas.DashboardApi.Controllers {
	public class ApiNotificationController : MungSecureController {
		public ApiNotificationController(IHostingEnvironment env) : base(env) {
		}

		[Route("api/notifications/test-settings")]
		public ActionResult NotificationTest(string toAddress, string fromAddress, string accessKey, string accessSecret) {
			var munger = CurrentUser;
			if (munger == null) {
				return Redirect("/login");
			}

			if (string.IsNullOrEmpty(toAddress)) {
				return Json(new { Success = false, Message = $"Please supply a 'To' email address" });

			}

			try {
				var success = SesNotification.TestConfiguration(toAddress, fromAddress,  accessKey, accessSecret);
				if (success) {
					return Json(new { Success = true, Message = "Email send success"});

				} else {
					return Json(new { Success = false, Message = "AWS SES didn't respond as expected, please check your keys and ensure that the From email address has been Authorized" });

				}
			}catch(Exception ex) {
				var aggEx = ex as AggregateException;

				if (aggEx!=null) {
					var exx = aggEx.InnerExceptions.FirstOrDefault();
					return Json(new { Success = false, Message = $"An exception occurred: {exx.Message}<br><br>{exx.StackTrace.Replace("\r\n", "</br>")}" });

				}
				return Json(new { Success = false, Message = $"An exception occurred: {ex.Message}<br><br>{ex.StackTrace.Replace("\r\n", "</br>")}" });

			}



		}

	}
}