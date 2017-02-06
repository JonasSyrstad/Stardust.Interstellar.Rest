using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
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
            Logging.DebugMessage($"Serving request from {Request.UserHostAddress}");
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
}
