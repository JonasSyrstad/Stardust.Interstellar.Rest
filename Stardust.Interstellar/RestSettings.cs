using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public static class RestSettings
    {
        internal static bool initialized = false;
        public static void Initialize(bool useRestAsDefault = true)
        {
            if(initialized) return;
            //Resolver.GetConfigurator().UnBind<IHttpControllerActivator>().AllAndBind().To<ControllerActivator>().SetTransientScope();
            //Resolver.GetConfigurator().UnBind<IHttpActionInvoker>().AllAndBind().To<ActionInvoker>().SetTransientScope();
            Resolver.GetConfigurator().Bind<IAuthenticationHandler>().To<AuthHandler>().SetTransientScope();
            Resolver.GetConfigurator().Bind<IHeaderHandler>().To<StardustHeaderHandler>("StardustHeaderHandler").SetTransientScope();
            Resolver.GetConfigurator().Bind<IErrorHandler>().To<StardustErrorHandler>().SetTransientScope();
            Resolver.GetConfigurator().Bind<ILogger>().To<LogWrapper>().SetSingletonScope();
            ExtensionsFactory.SetServiceLocator(new StardustServiceLocator());
            if (useRestAsDefault) ServiceContainerFactory.RegisterServiceFactoryAsDefault(new RestServiceContainerFactory());
            initialized = true;
        }

        public static void RegisterOAuthProvider<T>() where T : IOAuthTokenProvider
        {
            Resolver.GetConfigurator().Bind<IOAuthTokenProvider>().To<T>().SetTransientScope();
        }

        public static void HandleServiceAdditions(Action<Dictionary<string, object>> handler)
        {
            ExtrasHandler = handler;
        }

        internal static Action<Dictionary<string, object>> ExtrasHandler { get; set; }
    }

    public class LogWrapper : ILogger
    {
        public void Error(Exception error)
        {
            error.Log("rest generator");
        }

        public void Message(string message)
        {
            Logging.DebugMessage(message);
        }

        public void Message(string format, params object[] args)
        {
            Logging.DebugMessage(format,args);
        }
    }
}
