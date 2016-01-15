using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;

namespace GrowingData.Mung.Web.Areas.DashboardApi.Controllers {
	public class ApiFirehoseController : MungSecureController {
		public ApiFirehoseController(IHostingEnvironment env) : base(env) {
		}



		[HttpGet]
		[Route("api/firehose/poll")]
		public ActionResult Poll(string eventTypes) {
			using (var request = MungApp.Current.LongPollProcessor.Listen(eventTypes.Split(','))) {

				var mungEvent = request.WaitForEvent();
				return Json(new { Event = mungEvent, Success = true });
			}

		}

	}
}