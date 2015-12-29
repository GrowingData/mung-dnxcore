using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GrowingData.Mung.Web.Areas.Ingest.Controllers {
    public class HttpIngestController : Controller {


        [Route("ingest/json")]
        public IActionResult Index() {
            return Json(new { Message = "Success" });
        }
    }
}
