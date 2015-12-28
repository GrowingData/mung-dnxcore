﻿#if TESTING
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Identity;
using GrowingDataMungWeb.Mocks.Common;

namespace GrowingDataMungWeb.Mocks.Google
{
    internal class TestGoogleEvents
    {
        internal static Task OnCreatingTicket(OAuthCreatingTicketContext context)
        {
            if (context.Principal != null)
            {
                Helpers.ThrowIfConditionFailed(() => context.AccessToken == "ValidAccessToken", "Access token is not valid");
                Helpers.ThrowIfConditionFailed(() => context.RefreshToken == "ValidRefreshToken", "Refresh token is not valid");
                Helpers.ThrowIfConditionFailed(() => GoogleHelper.GetEmail(context.User) == "AspnetvnextTest@gmail.com", "Email is not valid");
                Helpers.ThrowIfConditionFailed(() => GoogleHelper.GetId(context.User) == "106790274378320830963", "Id is not valid");
                Helpers.ThrowIfConditionFailed(() => GoogleHelper.GetFamilyName(context.User) == "AspnetvnextTest", "FamilyName is not valid");
                Helpers.ThrowIfConditionFailed(() => GoogleHelper.GetName(context.User) == "AspnetvnextTest AspnetvnextTest", "Name is not valid");
                Helpers.ThrowIfConditionFailed(() => context.ExpiresIn.Value == TimeSpan.FromSeconds(1200), "ExpiresIn is not valid");
                Helpers.ThrowIfConditionFailed(() => context.User != null, "User object is not valid");
                context.Principal.Identities.First().AddClaim(new Claim("ManageStore", "false"));
            }

            return Task.FromResult(0);
        }

        internal static Task OnTicketReceived(TicketReceivedContext context)
        {
            if (context.Principal != null && context.Options.SignInScheme == new IdentityCookieOptions().ExternalCookieAuthenticationScheme)
            {
                //This way we will know all events were fired.
                var identity = context.Principal.Identities.First();
                var manageStoreClaim = identity?.Claims.Where(c => c.Type == "ManageStore" && c.Value == "false").FirstOrDefault();
                if (manageStoreClaim != null)
                {
                    identity.RemoveClaim(manageStoreClaim);
                    identity.AddClaim(new Claim("ManageStore", "Allowed"));
                }
            }

            return Task.FromResult(0);
        }

        internal static Task RedirectToAuthorizationEndpoint(OAuthRedirectToAuthorizationContext context)
        {
            context.Response.Redirect(context.RedirectUri + "&custom_redirect_uri=custom");
            return Task.FromResult(0);
        }
    }
} 
#endif