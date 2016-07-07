﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data.SqlClient;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {
    public class MungerIdentity : ClaimsIdentity {
        public const string OriginalIssuer = "mung.io";

        private Munger _user = null;
        public Munger User
        {
            get { return _user; }
        }

        public static string Issuer(HttpContext context) {
            return context.Request.Host.Value.ToLower();
        }

        public MungerIdentity(Munger user, HttpContext context)
            : base(CookieAuthenticationDefaults.AuthenticationScheme) {

            _user = user; 
            this.AddClaim(new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.String, Issuer(context), OriginalIssuer));
            this.AddClaim(new Claim(ClaimTypes.Name, user.Name, ClaimValueTypes.String, Issuer(context), OriginalIssuer));
        }


        public static MungerIdentity LoadIdentity(HttpContext context) {
            if (context.User.Identities.Any(identity => identity.IsAuthenticated)) {
                var emailClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                // Make sure that the claim is good.
                if (emailClaim == null || emailClaim.Issuer != Issuer(context) || emailClaim.OriginalIssuer != OriginalIssuer) {
                    return null;
                }

                var email = emailClaim.Value;
                var user = Munger.Get(email);

                if (user == null) {
                    return null;
                }

                var identity = new MungerIdentity(user, context);


                return identity;
            }
            return null;

        }
    }
}
