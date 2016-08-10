using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Interstellar.Rest.Client.Graph;
using Stardust.Interstellar.Rest.Client.Graph.InternalHelpers;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public static class GraphClientFactory
    {
        public static GraphContext<T> CreateGraphClient<T>(this IRuntime runtime) where T:IGraphContext, new()
        {
            var serviceName = GetServiceName<T>();
            return CreateGraphContext<T>(runtime, serviceName);
        }

        private static string GetServiceName<T>() where T : IGraphContext, new()
        {
            var serviceName = typeof(T).GetAttribute<ServiceNameAttribute>().ServiceName;
            if (serviceName.IsNullOrWhiteSpace()) serviceName = typeof(T).GenericTypeArguments.Single().Name;
            return serviceName;
        }

        public static GraphContext<T> CreateGraphContext<T>(this IRuntime runtime, string serviceName) where T : IGraphContext, new()
        {
            var context = new T();
            context.Initialize(runtime.Context.GetServiceConfiguration(serviceName).GetConfigParameter("Address"));
            var realContext = (context as object) as GraphContext<T>;
            if (realContext == null) throw new InvalidCastException(typeof(T).Name + " is not a graph context");
            return realContext;
        }

        public static GraphContext<T> CreateGraphContext<T>(this IRuntime runtime, string serviceName,Action<Dictionary<string, object>> extrasHandler) where T : IGraphContext, new()
        {
            var context = runtime.CreateGraphContext<T>(serviceName);
            context.SetExtrasHandlerInternal(extrasHandler);
            return context;
        }

        public static GraphContext<T> CreateGraphContext<T>(this IRuntime runtime,  Action<Dictionary<string, object>> extrasHandler) where T : IGraphContext, new()
        {
            return CreateGraphContext<T>(runtime, GetServiceName<T>(), extrasHandler);
        }
    }
}
