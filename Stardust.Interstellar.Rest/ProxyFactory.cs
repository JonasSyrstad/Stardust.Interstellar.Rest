using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Stardust.Interstellar.Rest
{
    public static class ProxyFactory
    {
        static ConcurrentDictionary<Type,Type> proxyTypeCache=new ConcurrentDictionary<Type, Type>();
        public static Type CreateProxy<T>()
        {
            Type type;
            if (proxyTypeCache.TryGetValue(typeof(T), out type)) return type;
            var builder = new ProxyBuilder<T>();
            var newType= builder.Build();
            if (proxyTypeCache.TryGetValue(typeof(T), out type)) return type;
            proxyTypeCache.TryAdd(typeof(T), newType);
            return newType;
        }

        public static T CreateInstance<T>(string baseUrl)
        {
            var t = CreateProxy<T>();
            var auth = typeof(T).GetCustomAttributes().SingleOrDefault(a => a is IAuthenticationInspector) as IAuthenticationInspector;
            var authHandler = GetAuthenticationHandler<T>(auth);
            var instance = Activator.CreateInstance(t, authHandler, new HeaderHandlerFactory(typeof(T)), TypeWrapper.Create<T>());
            var i = (RestWrapper)instance;
            i.SetBaseUrl(baseUrl);
            return (T)instance;
        }

        private static IAuthenticationHandler GetAuthenticationHandler<T>(IAuthenticationInspector auth)
        {
            IAuthenticationHandler authHandler;
            if (auth == null) authHandler = new NullAuthHandler();
            else
            {
                authHandler = auth.GetHandler();
            }
            return authHandler;
        }
    }
}