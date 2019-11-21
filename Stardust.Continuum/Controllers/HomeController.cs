using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Stardust.Core.Service.Web.Identity.Passive;
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Continuum.Controllers
{
    public class HomeController : Controller
    {
        static HomeController()
        {
            
            sources = new ConcurrentDictionary<string, string>();
            sources.TryAdd("-Select source-", "-Select source-");
            sources.TryAdd("All", "All");
        }
        internal static ConcurrentDictionary<string, string> sources;

        internal static List<string> itemd = new List<string> { "-Select source-", "All" };
        internal static List<string> itemc = new List<string> { "-Select source-", "Total" };
        public ActionResult Index()
        {
            if (!ConfigurationManagerHelper.GetValueOnKey("authority").IsNullOrWhiteSpace() &&
                !User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            Logging.DebugMessage($"Serving request from {Request.UserHostAddress}");
            if (ConfigurationManagerHelper.GetValueOnKey("allowedRoles","").ContainsCharacters())
            {
                var roles=(User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == "roles").ToList();
                if (roles.All(c => c.Value != ConfigurationManagerHelper.GetValueOnKey("allowedRoles", "")))
                {
                    throw new UnauthorizedAccessException("Unauthorized access");
                }
            }
            try
            {
                ViewBag.Title = "Log stream";

                ViewBag.Sources = itemd;
                return View();
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }

        }

        public ActionResult About()
        {
            if (!ConfigurationManagerHelper.GetValueOnKey("authority").IsNullOrWhiteSpace() &&
                !User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            Logging.DebugMessage($"Serving request from {Request.UserHostAddress}");
            try
            {
                ViewBag.Title = "About the continuum";

                var s=   new ConcurrentDictionary<string, string>();
                s.TryAdd("-Select source-", "-Select source-");
                s.TryAdd("Total", "Total");
                ViewBag.Sources = itemc;
                return View();
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }

        }
    }

    public class AuthController : Controller
    {
        public  ActionResult Callback(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
            {
                IsPersistent = true
            }, (ClaimsIdentity)HttpContext.GetOwinContext().Authentication.User.Identity);
            if (returnUrl.IsNullOrWhiteSpace()) returnUrl = "/";
            return Redirect(returnUrl);
        }

        public  ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) RedirectToAction("index", "home");
            return new ChallengeResult(OpenIdConnectAuthenticationDefaults.AuthenticationType, returnUrl.ContainsCharacters() ? returnUrl : Url.Action("index", "home"));
        }

        public virtual ActionResult Signout(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.SignOut(HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes().Select(t => t.AuthenticationType).ToArray());
            return RedirectToAction(returnUrl);
        }

        public ActionResult UnAuth()
        {
            return View();
        }
        [System.Web.Http.Authorize]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Renew(string ourl)
        {
            return Redirect(ourl);
        }
    }
}
