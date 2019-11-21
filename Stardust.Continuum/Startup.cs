using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using WebGrease.Configuration;

[assembly: OwinStartup(typeof(Stardust.Continuum.Startup))]

namespace Stardust.Continuum
{
    public partial class Startup
    {
        public static IHubContext hub;

        public void Configuration(IAppBuilder app)
        {

            ConfigureAuth(app);
            app.MapSignalR("/signalr", new HubConfiguration
            {
                EnableJSONP = true,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true,

            });
            if (!ConfigurationManagerHelper.GetValueOnKey("authority").IsNullOrWhiteSpace())
                GlobalHost.HubPipeline.RequireAuthentication();
            hub = GlobalHost.ConnectionManager.GetHubContext<StreamHub>();


        }
    }
}
