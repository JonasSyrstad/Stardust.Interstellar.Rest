using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Core.Service.Web;
using Stardust.Interstellar.Rest.Legacy;
using Stardust.Interstellar.Rest.Service;
using Stardust.Interstellar.Rest.Test;
using Stardust.Particles;

namespace Stardust.Interstellar.Test
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WcfServiceProvider.RegisterWcfAdapters();   
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ITestApi>();
            ServiceFactory.FinalizeRegistration();

            this.LoadBindingConfiguration<TestBlueprint>();
           
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            
            Logging.Exception(Server.GetLastError());
        }
    }

    public class TestBlueprint:Blueprint
    {
        /// <summary>Place your bindings here</summary>
        protected override void DoCustomBindings()
        {
            base.DoCustomBindings();
            Configurator.Bind<ITestApi>().To<TestApiImp>().SetTransientScope();
            Configurator.Bind<IWcfWrapper>().To<WcfWrapper>().SetTransientScope();
        }
    }

    public class WcfWrapper : IWcfWrapper
    {
        public StringWrapper TestImplementationGet(string wrapper)
        {
            return new StringWrapper {Value = $"Hello {wrapper}" };
        }

        public StringWrapper TestImplementationPut(string id, StringWrapper wrapper)
        {
           return new StringWrapper { Value = $"{id}: {wrapper.Value}" };
        }
    }

    public class TestApiImp : ITestApi
    {
        public string Apply1(string id, string name)
        {
            return string.Join("-", id, name);
        }

        public string Apply2(string id, string name, string item3)
        {
            return string.Join("-", id, name,item3);
        }

        public string Apply3(string id, string name, string item3, string item4)
        {
            return string.Join("-", id, name,item3,item4);
        }

        public void Put(string id, DateTime timestamp)
        {
            return;
        }

        public Task<StringWrapper> ApplyAsync(string id, string name, string item3, string item4)
        {
            return Task.FromResult(new StringWrapper {Value = string.Join("-", id, name, item3, item4) });
        }

        public Task PutAsync(string id, DateTime timestamp)
        {
            return Task.FromResult(2);
        }

        public Task FailingAction(string id, string timestamp)
        {
            throw new NotImplementedException();
        }
    }
}
