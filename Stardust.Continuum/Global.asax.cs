using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Core.Default.Implementations;
using Stardust.Interstellar.Rest.Service;
using Stardust.Core.Service.Web;
using Stardust.Interstellar;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Stardust.Continuum.Client;
using Stardust.Particles;

namespace Stardust.Continuum
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.LoadBindingConfiguration<ContinuumBlueprint>();
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ILogStream>();
            ServiceFactory.FinalizeRegistration();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                
                args.SetObserved();
                args.Exception.Handle(e =>
                {
                    var aggregate = e as AggregateException;
                    if (aggregate != null)
                    {
                        foreach (var aggregateInnerException in aggregate.InnerExceptions)
                        {
                            aggregateInnerException.Log("TaskException");
                        }
                    }
                    else e.Log("TaskException");
                    return true;
                });
            };
        }
    }

    public class ContinuumBlueprint : Blueprint<SelfLogger>
    {
        protected override void DoCustomBindings()
        {
            base.DoCustomBindings();
            Configurator.Bind<ILogStream>().To<StreamServiceImp>().SetSingletonScope();
        }
    }

    public class SelfLogger : ILogging
    {
        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            Task.Run(() =>
            {
                try
                {
                    var item = new StreamItem
                    {
                        Timestamp = DateTime.UtcNow,
                        Message =
                                    exceptionToLog.Message +
                                    (string.IsNullOrWhiteSpace(additionalDebugInformation) ? "" : $"[{additionalDebugInformation}]"),
                        LogLevel = LogLevels.Error,
                        CorrelationToken = Environment.MachineName,
                        UserName = "streamServer",
                        ServiceName = "Continuum",
                        StackTrace = exceptionToLog.StackTrace
                    };
                    PushMessage(item);
                }
                catch 
                {
                    
                }
            });
        }

        public void HeartBeat()
        {
            Task.Run(() =>
            {
                try
                {
                    var item = new StreamItem
                    {
                        Timestamp = DateTime.UtcNow,
                        Message = "BomBom",
                        LogLevel = LogLevels.Information,
                        CorrelationToken = Environment.MachineName,
                        UserName = "streamServer",
                        ServiceName = "Continuum",
                    };
                    PushMessage(item);
                }
                catch 
                {
                }
            });
        }

        public void DebugMessage(string message, EventLogEntryType entryType = EventLogEntryType.Information, string additionalDebugInformation = null)
        {
            Task.Run(() =>
            {
                try
                {
                    var item = new StreamItem
                    {
                        Timestamp = DateTime.UtcNow,
                        Message = message +
                                    (string.IsNullOrWhiteSpace(additionalDebugInformation) ? "" : $"[{additionalDebugInformation}]"),
                        LogLevel = LogLevels.Error,
                        CorrelationToken = Environment.MachineName,
                        UserName = "streamServer",
                        ServiceName = "Continuum"
                    };
                    PushMessage(item);
                }
                catch 
                {
                }
            });
        }

        public void SetCommonProperties(string logName)
        {
            Task.Run(() =>
            {
                try
                {
                    var item = new StreamItem
                    {
                        Timestamp = DateTime.UtcNow,
                        Message = "setting prop: " + logName,
                        LogLevel = LogLevels.Error,
                        CorrelationToken = Environment.MachineName,
                        UserName = "streamServer",
                        ServiceName = "Continuum"
                    };
                    PushMessage(item);
                }
                catch
                {
                    
                }
            });
        }

        private static void PushMessage(StreamItem item)
        {
            Startup.hub.Clients.Groups(new List<string> {"All", $"the.continuum"}).cmessage(item);
        }
    }
}
