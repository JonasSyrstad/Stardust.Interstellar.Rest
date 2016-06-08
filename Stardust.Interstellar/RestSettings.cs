using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Interstellar.Serializers;
using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    public static class RestSettings
    {
        internal static bool initialized = false;
        public static void Initialize(bool useRestAsDefault = true)
        {
            Resolver.GetConfigurator().Bind<IAuthenticationHandler>().To<AuthHandler>().SetTransientScope();
            Resolver.GetConfigurator().Bind<IHeaderHandler>().To<StardustHeaderHandler>("StardustHeaderHandler");
            Resolver.GetConfigurator().Bind<IErrorHandler>().To<StardustErrorHandler>();
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
}
