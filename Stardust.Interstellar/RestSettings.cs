using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Newtonsoft.Json.Linq;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    public static class RestSettings
    {
        internal static bool initialized = false;
        public static void Initialize(bool useRestAsDefault = true)
        {
            if(initialized) return;
           
            Resolver.GetConfigurator().Bind<IAuthenticationHandler>().To<AuthHandler>().SetTransientScope();
            Resolver.GetConfigurator().Bind<IHeaderHandler>().To<StardustHeaderHandler>("StardustHeaderHandler").SetTransientScope();
            Resolver.GetConfigurator().Bind<IErrorHandler>().To<StardustErrorHandler>().SetTransientScope();
            Resolver.GetConfigurator().Bind<ILogger>().To<LogWrapper>().SetSingletonScope();
            Resolver.GetConfigurator().Bind<IStateCache>().To<StardustMessageContainer>().SetSingletonScope();
            ExtensionsFactory.SetServiceLocator(new StardustServiceLocator());
            if (useRestAsDefault) ServiceContainerFactory.RegisterServiceFactoryAsDefault(new RestServiceContainerFactory());
            initialized = true;
        }

        public static void SetControllerActivation()
        {
            Resolver.GetConfigurator().UnBind<IHttpControllerActivator>().AllAndBind().To<ControllerActivator>().SetTransientScope();
            Resolver.GetConfigurator().UnBind<IHttpActionInvoker>().AllAndBind().To<ActionInvoker>().SetTransientScope();
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

    public class StardustMessageContainer : IStateCache
    {
        public JObject GetState()
        {
            return ContainerFactory.Current.Resolve(typeof(JObject), Scope.Context) as JObject;
        }

        public void SetState(JObject extendedMessage)
        {
            ContainerFactory.Current.Bind(typeof(JObject),extendedMessage,Scope.Context);
        }
    }
}
