# Stardust.Interstellar.Rest

This is a tool for creating webapi controllers and clients based upon an interface that is mostly decorated by the common annotations you are used to. It is mainly intended to be used inside the organization 0r the product. But workes well to provide .net clients to the public as well.

When both the client and the service are located in the same solution you will have ctrl-F12 support from the client code.

Unit testing and reuse are simplified. All controllers are implemented in an consistent way, no surprises.

Add the nuget: Install-Package Stardust.Interstellar.Rest

Sample service definition interface
```CS
    [IRoutePrefix("api")] //Custom: as the RoutePrefix attribute only supports classes, this is one to one
    [CallingMachineName]
    public interface ITestApi
    {
        [Route("test/{id}")]
        [HttpGet]
        string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);// the In attribute supports more than the basic webapi attributes (but you can use them; FomeUri and FromBody)

        [Route("test2/{id}")]
        [HttpGet]
        string Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

        [Route("test3/{id}")]
        [HttpGet]
        string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

        [Route("put1/{id}")]
        [HttpPut]
        void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("test5/{id}")]
        [HttpGet]
        Task<StringWrapper> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

        [Route("put2/{id}")]
        [HttpPut]
        Task PutAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);
    }
```
Creating a service proxy
```CS

        var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/",
                    extras =>
                        {
                            foreach (var extra in extras)
                            {
                                output.WriteLine($"{extra.Key}:{extra.Value}");
                            }
                        });
        try
        {
            var res =await service.ApplyAsync("101", "SampleService", "Hello", "Sample");
            output.WriteLine(res.Value);
            // outputs: 101-SampleService-Hello-Sample
        }
        catch (Exception ex)
        {
            throw;
        }
```
Optional: Setting JsonSerializerSettings to service definitions or message types 

```CS

new JsonSerializerSettings
                              {
                                  DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                                  DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                  NullValueHandling = NullValueHandling.Include,
                                  Formatting = Formatting.Indented
                              }.AddClientSerializer<IMyService>().AddClientSerializer<SomeMessageType>;
```


Creating the service implementation (just implement it as you implement any other interface):
```CS
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
    }
```

Creating the WebApi controller
```CS
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ITestApi>();
            ServiceFactory.FinalizeRegistration();

            this.LoadBindingConfiguration<TestBlueprint>();
           
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
```
Adding swagger to the service add the following line in Swashbuckle's SwaggerConfig.cs (added with the nuget)
Add the nuget: Install-Package Stardust.Interstellar.Swashbuckle

```CS
    c.ConfigureStardust();
```
Creating client extensions
```CS
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Interface)]
    public sealed class CallingMachineNameAttribute:HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] {new CallingMachineNameHandler()};
        }
    }

    public class CallingMachineNameHandler : IHeaderHandler
    {
        public void SetHeader(HttpWebRequest req)
        {
            req.Headers.Add("x-callingMachine",Environment.MachineName);
        }
    }
```
Statefull extesions can be created like this:
```CS
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class PerformanceHeadersAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] { new PerformanceHeadersHandler() };
        }
    }



    internal class PerformanceHeadersHandler : StatefullHeaderHandlerBase
    {
        private const string StardustTimerKey = "x-stardusttimer";

        protected override void DoSetHeader(StateDictionary state, HttpWebRequest req)
        {
            state.SetState(StardustTimerKey, Stopwatch.StartNew());//add to state container
        }

        protected override void DoGetHeader(StateDictionary state, HttpWebResponse response)
        {
            var sw = state.GetState<Stopwatch>(StardustTimerKey);//get from state container
            sw.Stop();
            var server = response.Headers[StardustTimerKey];
            if (!string.IsNullOrWhiteSpace(server))
            {
                var serverTime = long.Parse(server);
                var latency = sw.ElapsedMilliseconds - serverTime;
                state.Extras.Add("latency",latency);//add to client output
                state.Extras.Add("serverTime",serverTime);
                state.Extras.Add("totalTime",sw.ElapsedMilliseconds);
            }

        }

        protected override void DoSetServiceHeaders(StateDictionary state, HttpResponseHeaders headers)
        {
            var sw =  state.GetState<Stopwatch>(StardustTimerKey);
            sw.Stop();
            headers.Add(StardustTimerKey, sw.ElapsedMilliseconds.ToString());
        }

        protected override void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers)
        {
            state.Add(StardustTimerKey, Stopwatch.StartNew());
        }
    }
```

To wrap existing wcf services as WebApi services:
```CS
protected void Application_Start()
        {
            WcfServiceProvider.RegisterWcfAdapters();   //Adds the wcf adapter package to the generator
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ITestApi>(); //Same as before
            ServiceFactory.FinalizeRegistration();

            this.LoadBindingConfiguration<TestBlueprint>();
           
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
``` 

Creating a graph api and client

The graph context:
```CS
public class GraphTestApi:GraphContext<Employee>
    {
        public GraphTestApi(string baseUrl) : base(baseUrl)
        {
            Id = "grabUserIdFromIdentity";
        }
        public IGraphCollection<Employee> Employees
        {
            get
            {
                return CreateGraphCollection<Employee>();
            }
        }

        public IGraphItem<Employee> Me
        {
            get
            {
                return CreateGraphItem<Employee>(Id);
            }
        }
    }
```
and the Employee data type:

```CS
public class Employee : GraphBase
    {
        private string name;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string Email { get; set; }

        [JsonIgnore]
        public Employee Manager
        {
            get
            {
                return CreateGraphItem<Employee>(ManagerId).Value;
            }
        }

        public async Task<Employee> GetManagerAsync()
        {
            return await CreateGraphItem<Employee>(ManagerId).GetAsync();
        }

        [JsonIgnore]
        public IGraphCollection<Employee> Colleagues
        {
            get
            {
                
                return CreateGraphCollection<Employee>("colleagues", name);
            }
        }

        public string ManagerId { get; set; }
    }
```

To add support for WCF rest annotations:
Install the nuget: Install-Package Stardust.Interstellar.Rest.Legacy

and call the wcf addon before creating the controllers:
```CS
 public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WcfServiceProvider.RegisterWcfAdapters();//adds the wcf addon
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ITestApi>();//generates the controllers
            ServiceFactory.FinalizeRegistration(); /registers the new controllers with mvc webapi

            this.LoadBindingConfiguration<TestBlueprint>();
           
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

```

See [Stardust.KeenIo.Client](https://github.com/JonasSyrstad/Stardust.KeenIo.Client "Stardust.KeenIo.Client") for a demo/sample project on building .net client api's for existing rest services. It currenly supports Adding events and getting collection info from the api.
