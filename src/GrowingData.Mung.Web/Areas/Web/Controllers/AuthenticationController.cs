using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Auth.Controllers {

	[Area("Web")]
	public class AuthenticationController : Controller {


		[Route(Urls.LOGIN)]
		[HttpGet]
		public ActionResult Login() {

			// Make sure that we send them to the Setup page if there are no accounts
			if (!Munger.HasMungers) {
				return Redirect($"/{Urls.INITIAL_ACCOUNT_CREATE}");
			}

			return View();
		}

		[Route(Urls.LOGIN)]
		[HttpPost]
		public ActionResult Login(string email, string password) {


			var munger = Munger.Get(email);
			if (munger == null) {

				MungApp.Current.ProcessInternalEvent("login_failure", new {
					email = email,
					reason = "wrong_email",
					munger_id = -1
				});

				ViewBag.ErrorMessage = "Please check your email / password.";
				return View();
			}


			var inputHash = StringHashing.HashStrings(password, munger.PasswordSalt);

			if (inputHash != munger.PasswordHash) {
				MungApp.Current.ProcessInternalEvent("login_failure", new {
					email = email,
					reason = "wrong_password",
					munger_id = munger.MungerId
				});

				ViewBag.ErrorMessage = "Please check your email / password.";
				return View();
			}
			HttpContext.Login(munger);


			MungApp.Current.ProcessInternalEvent("login_success", new {
				email = email,
				munger_id = munger.MungerId
			});

			return Redirect("/" + Urls.DEFAULT);


		}

		[Route(Urls.LOGOUT)]
		[HttpPost]
		public ActionResult Logout() {

			HttpContext.Logout();
			return Redirect("/" + Urls.DEFAULT);
		}
	}
}