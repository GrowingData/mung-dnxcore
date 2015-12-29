using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

using System.Security.Claims;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Authentication.Cookies;
using System.Data.SqlClient;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {
    public class MungerIdentity : ClaimsIdentity {
        public const string OriginalIssuer = "mung.io";

        private Munger _munger = null;
        public Munger Munger
        {
            get { return _munger; }
        }

        public static string Issuer(HttpContext context) {
            return context.Request.Host.Value.ToLower();
        }

        public MungerIdentity(Munger user, HttpContext context)
            : base(CookieAuthenticationDefaults.AuthenticationScheme) {

            _munger = user; 
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
