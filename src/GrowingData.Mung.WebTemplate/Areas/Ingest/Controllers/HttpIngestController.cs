using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Cors;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Caching.Memory;
using GrowingData.Mung.WebTemplate.Models;
using GrowingData.Mung.WebTemplate.ViewModels;

namespace GrowingData.Mung.WebTemplate.Areas.Admin.Controllers {
    [Area("Ingest")]
    public class HttpIngestController : Controller {

        [HttpGet]
        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> Test(string albumName) {

            await Task.Run(() => Console.WriteLine("Test, doing someting async too!"));

            return Json(new { Message = "Success!" });
        }
    }
}