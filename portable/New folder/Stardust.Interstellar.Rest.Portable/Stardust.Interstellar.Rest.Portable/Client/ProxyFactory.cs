using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Client
{
    public static class ProxyFactory
    {
        static ConcurrentDictionary<Type, Type> proxyTypeCache = new ConcurrentDictionary<Type, Type>();

        public static bool EnableExpectContinue100ForPost { get; set; }

        public static bool EnableExpectContinue100ForAll { get; set; }

        public static Type CreateProxy<T>()
        {
            var interfaceType = typeof(T);
            return CreateProxy(interfaceType);
        }

        public static Type CreateProxy(Type interfaceType)
        {
            Type type;
            if (proxyTypeCache.TryGetValue(interfaceType, out type)) return type;
            lock (interfaceType)
            {
                if (proxyTypeCache.TryGetValue(interfaceType, out type)) return type;
                var builder = new ProxyBuilder(interfaceType);
                var newType = builder.Build();
                if (proxyTypeCache.TryGetValue(interfaceType, out type)) return type;
                proxyTypeCache.TryAdd(interfaceType, newType);
                return newType;
            }
        }

        public static T CreateInstance<T>(string baseUrl)
        {
            return CreateInstance<T>(baseUrl, null);
        }

        public static T CreateInstance<T>(string baseUrl, Action<Dictionary<string, object>> extrasCollector)
        {
            return (T)CreateInstance(typeof(T), baseUrl, extrasCollector);
        }

        public static object CreateInstance(Type interfaceType, string baseUrl)
        {
            return CreateInstance(interfaceType, baseUrl, null);
        }
        public static object CreateInstance(Type interfaceType, string baseUrl, Action<Dictionary<string, object>> extrasCollector)
        {
            var t = CreateProxy(interfaceType);
            var auth = interfaceType.GetCustomAttributes().SingleOrDefault(a => a is IAuthenticationInspector) as IAuthenticationInspector;
            var authHandler = GetAuthenticationHandler(auth);
            var instance = Activator.CreateInstance(t, authHandler, new HeaderHandlerFactory(interfaceType), TypeWrapper.Create(interfaceType));
            ((RestWrapper)instance).Extras = extrasCollector;
            var i = (RestWrapper)instance;
            i.SetBaseUrl(baseUrl);
            return instance;
        }

        private static object GetAuthenticationHandler(IAuthenticationInspector auth)
        {
            IAuthenticationHandler authHandler;
            if (auth == null)
            {
                authHandler = ExtensionsFactory.GetService<IAuthenticationHandler>() ?? new NullAuthHandler();
            }
            else
            {
                authHandler = auth.GetHandler();
            }
            return authHandler;
        }

    }
}