using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Core.Service.Web;
using Stardust.Interstellar.Rest.Client.Graph;
using Stardust.Interstellar.Rest.Common;
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
            this.LoadBindingConfiguration<TestBlueprint>();
            WcfServiceProvider.RegisterWcfAdapters();
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ITestApi>();
            ServiceFactory.FinalizeRegistration();



            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            Logging.Exception(error, "Unhandled exception....");
        }

    }

    public class TestBlueprint : Blueprint
    {
        /// <summary>Place your bindings here</summary>
        protected override void DoCustomBindings()
        {
            base.DoCustomBindings();
            Configurator.Bind<ITestApi>().To<TestApiImp>().SetTransientScope();
            Configurator.Bind<IWcfWrapper>().To<WcfWrapper>().SetTransientScope();
            Configurator.Bind<IEmployeeService>().To<EmployeeService>();
            Configurator.Bind<ILogger>().To<LogWrapper>().SetSingletonScope();
        }
    }

    public class EmployeeService : IEmployeeService
    {
        static EmployeeService()
        {
            employees.Add("jonassyrstad@outlook.com", new Employee { Email = "jonassyrstad@outlook.com", ManagerId = "mehh@outlook.com", Name = "Jonas Syrstad" });
            employees.Add("mehh@outlook.com", new Employee { Email = "mehh@outlook.com", ManagerId = "", Name = "Duche" });
            employees.Add("jonas.syrstad@dnvgl.com", new Employee { Email = "jonas.syrstad@dnvgl.com", ManagerId = "mehh@outlook.com", Name = "my alternate self" });
        }


        private static Dictionary<string, Employee> employees = new Dictionary<string, Employee>();
        public Task<Employee> GetAsync(string id)
        {
            return Task.FromResult(employees[id]);
        }

        public Task<IEnumerable<Employee>> QueryAsync(GraphQuery queryExpression)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Employee>> GetAllAsync()
        {
            return Task.FromResult(employees.Select(e => e.Value));
        }

        public Task AddAsync(Employee item)
        {
            employees.Add(item.Email, item);
            return Task.FromResult(1);
        }

        public Task RemoveAsync(string id)
        {
            employees.Remove(id);
            return Task.FromResult(1);
        }

        public Task UpdateAsync(string id, Employee item)
        {
            employees.Remove(id);
            employees.Add(item.Email, item);
            return Task.FromResult(1);
        }

        public Task<IEnumerable<Employee>> GetGraphNodesAsync(string id, string graphNodes)
        {
            return Task.FromResult(employees.Where(e => !string.IsNullOrWhiteSpace(e.Value.ManagerId)).Select(e => e.Value));
        }
    }

    public class WcfWrapper : IWcfWrapper
    {
        public StringWrapper TestImplementationGet(string wrapper)
        {
            return new StringWrapper { Value = $"Hello {wrapper}" };
        }

        public StringWrapper TestImplementationPut(string id, StringWrapper wrapper)
        {
            foreach (var msg in this.GetExtendedMessage())
            {
                Logging.DebugMessage($"{msg.Key} +  {msg.Value}");
            }
            return new StringWrapper { Value = $"{id}: {wrapper.Value}" };
        }

        public StringWrapper TestImplementationPut2(string id, IDictionary<string, IEnumerable<object>> hierarcy)
        {
            Logging.DebugMessage(this.GetExtendedMessage().ToString());
            return new StringWrapper {Value = "OK"};
        }
    }

    public class TestApiImp : ITestApi
    {
        public string Apply1(string id, string name)
        {
            return string.Join("-", id, name);
        }

        public Task<StringWrapper> Apply2(string id, string name, string item3)
        {
            return Task.FromResult(new StringWrapper { Value = string.Join("-", id, name, item3) });
        }

        public string Apply3(string id, string name, string item3, string item4)
        {
            return string.Join("-", id, name, item3, item4);
        }

        public void Put(string id, DateTime timestamp)
        {
            return;
        }

        public async Task<StringWrapper> ApplyAsync(string id, string name, string item3, string item4)
        {
            //await Task.Run(() => {
            //    throw new Exception("test"); 
            //});
            var rnd = new Random();
            if (rnd.Next(0, 3) == 2) throw new StatusException(HttpStatusCode.RequestTimeout);
            return new StringWrapper { Value = string.Join("-", id, name, item3, item4) };
            //return Task.FromResult(new StringWrapper {Value = string.Join("-", id, name, item3, item4) });
        }

        public Task PutAsync(string id, DateTime timestamp)
        {
            return Task.FromResult(2);
        }

        public Task FailingAction(string id, string timestamp)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetOptions()
        {
            return Task.FromResult(new List<string> { "Stardust", "Interstellar", "Rest", "2.4.*" });
        }

        public Task GetHead()
        {
            return Task.FromResult(new List<string> { "Stardust", "Interstellar", "Rest", "2.4.*" });
        }
    }
}
