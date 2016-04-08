using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Cors;

namespace GrowingData.Mung.Web.Areas.DashboardApi.Controllers {
	public class ApiFirehoseController : MungSecureController {
		public ApiFirehoseController(IHostingEnvironment env) : base(env) {
		}



		[HttpGet]
		[EnableCors("Any")]
		[Route("api/firehose/poll")]
		public ActionResult Poll(string eventTypes) {
			using (var request = MungApp.Current.LongPollProcessor.Listen(eventTypes.Split(','))) {

				var mungEvent = request.WaitForEvent(5000);
				return Json(new { Event = mungEvent, Success = true });
			}

		}

	}
}