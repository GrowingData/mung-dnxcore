using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GrowingData.Utilities;
using GrowingData.Mung.Web;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.Areas.Auth.Controllers {

	[Area("Web")]
	public class InviteController : Controller {
        // GET: Auth/Authenication
        [Route("invite")]
        [HttpGet]
        public ActionResult Invite(string code) {
            return View();
        }

        [Route("invite")]
        [HttpPost]
        public ActionResult Invite(string email, string name) {

            var existing = Munger.Get(email);
            if (existing != null) {
                ViewBag.ErrorMessage = "A user with the same email has already created an account";
                return View();
            }

            var munger = new Munger() {
                Name = name,
                Email = email,
                PasswordSalt = null, // salt,
                PasswordHash = null //passwordHash
            };

            munger.Insert();

            HttpContext.Login(munger);

            return Redirect(Urls.DEFAULT);

        }

        [Route("invite/response")]
        [HttpGet]
        public ActionResult InvitationResponse() {
            return View();
        }

        [Route("invite/response")]
        [HttpPost]
        public ActionResult InvitationResponse(string email, string password, string name) {
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
                PasswordHash = passwordHash
            };

            munger.Insert();

            HttpContext.Login(munger);

            return Redirect(Urls.DEFAULT);

        }
    }
}