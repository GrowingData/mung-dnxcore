﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GrowingData.Utilities;
using GrowingData.Mung.Web;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Auth.Controllers {
	[Area("Web")]
	public class SetupController : Controller {

		// GET: Auth/Authenication
		[Route(Urls.INITIAL_ACCOUNT_CREATE)]
		[HttpGet]
		public ActionResult Setup() {
			if (Munger.HasMungers) {
				return Redirect("/");
			}
			return View();
		}

		[Route(Urls.INITIAL_ACCOUNT_CREATE)]
		[HttpPost]
		public ActionResult Setup(string email, string password, string name) {
			if (Munger.HasMungers) {
				return Redirect("/");
			}
			return CreateUser(email, password, name, !Munger.HasMungers);
		}



		private ActionResult CreateUser(string email, string password, string name, bool isAdmin) {
			var salt = StringHashing.CreateSalt();
			var passwordHash = StringHashing.HashStrings(password, salt);

			var existing = Munger.Get(email);
			if (existing != null) {
				ViewBag.ErrorMessage = "A user with the same email has already created an account";
				return View();
			}

			var munger = new Munger() {
				Name = name,
				Email = email,
				PasswordSalt = salt,
				PasswordHash = passwordHash,
				IsAdmin = isAdmin
			};

			munger.Insert();

			HttpContext.Login(munger);


			return Redirect("/" + Urls.DEFAULT);
		}





	}
}