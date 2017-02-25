using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Stardust.Continuum.Models;
using Stardust.Particles;

namespace Stardust.Continuum
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                ExpireTimeSpan = TimeSpan.FromDays(90),
                AuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
                AuthenticationMode = AuthenticationMode.Active,
                CookieName = ".id",
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.Always,
                CookieHttpOnly = true,

                Provider = new CookieAuthenticationProvider
                {

                    OnValidateIdentity = context =>
                    {
                        context.ReplaceIdentity(context.Identity);
                        context.Properties.IsPersistent = true;
                        HttpContext.Current.User = new ClaimsPrincipal(context.Identity);
                        return Task.FromResult(0);
                    },
                    OnResponseSignIn = context =>
                    {
                        if (context == null) return;
                        if (context.CookieOptions != null) context.CookieOptions.Expires = DateTime.UtcNow.AddDays(90);
                        if (context.Properties != null)
                        {
                            context.Properties.IsPersistent = true;
                            context.Properties.ExpiresUtc = DateTime.UtcNow.AddDays(90);
                        }
                        //Logging.DebugMessage(context.AuthenticationType);
                    }
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            
            
            if (ConfigurationManagerHelper.GetValueOnKey("authority").IsNullOrWhiteSpace()) return;
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Passive,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie,

                ClientId = ConfigurationManagerHelper.GetValueOnKey("clientId"),
                Authority = ConfigurationManagerHelper.GetValueOnKey("authority"),
                //MetadataAddress = identitySettings.MetadataUrl.StartsWith("https://") ? identitySettings.MetadataUrl : "https://" + identitySettings.MetadataUrl ,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = context =>
                    {
                        if (context.OwinContext.Request.User.Identity.IsAuthenticated)
                        {
                            return Task.FromResult(0);
                        }
                        var code = context.Code;

                        return Task.FromResult(0);
                    }

                }
            });
        }
    }
}
