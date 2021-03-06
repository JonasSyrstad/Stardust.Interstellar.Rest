using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Annotations.Messaging;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Common
{
    public static class ExtensionsFactory
    {



        internal static StateDictionary GetState(this ResultWrapper result)
        {
            var actionId = result.ActionId;
            return StateHelper.InitializeState(actionId);
        }


        private static IServiceLocator locator;

        private static Type xmlSerializer;

        public static void SetServiceLocator(IServiceLocator serviceLocator)
        {
            locator = serviceLocator;
        }

        internal static IServiceLocator GetLocator()
        {
            return locator;
        }

        public static T GetService<T>()
        {
            return locator != null ? locator.GetService<T>() : default(T);
        }

        public static IEnumerable<T> GetServices<T>()
        {
            if (xmlSerializer != null && typeof(ISerializer) == typeof(T)) return new[] { (T)Activator.CreateInstance(xmlSerializer) };
            return locator?.GetServices<T>();
        }

        internal static string GetServiceTemplate(MethodInfo methodInfo)
        {
            var template = GetService<IRouteTemplateResolver>()?.GetTemplate(methodInfo);
            if (!String.IsNullOrWhiteSpace(template)) return template;
            var templateAttrib = methodInfo.GetCustomAttribute<RouteAttribute>();
            if (templateAttrib == null) return template;
            template = templateAttrib.Template;
            return template;
        }

        internal static string GetRouteTemplate(IRoutePrefixAttribute templatePrefix, RouteAttribute template, MethodInfo methodInfo)
        {
            var interfaceType = methodInfo.DeclaringType;
            var templateResolver =
                GetService<IRouteTemplateResolver>();
            var route = templateResolver?.GetTemplate(methodInfo);
            if (!String.IsNullOrWhiteSpace(route)) return route;
            string prefix = "";
            if (templatePrefix != null)
            {
                prefix = templatePrefix.Prefix;
                if (templatePrefix.IncludeTypeName) prefix = prefix + "/" + (interfaceType.GetGenericArguments().Any() ? interfaceType.GetGenericArguments().FirstOrDefault()?.Name.ToLower() : interfaceType.GetInterfaces().FirstOrDefault()?.GetGenericArguments().First().Name.ToLower());

            }
            return( templatePrefix == null ? "" : (prefix + "/") + template.Template).Replace("//","/");
        }

        internal static void BuildParameterInfo(MethodInfo methodInfo, ActionWrapper action)
        {
            
            var parameterHandler = GetService<IServiceParameterResolver>();
            if (parameterHandler != null)
            {
                var parameters = parameterHandler.ResolveParameters(methodInfo);
                if (parameters != null && parameters.Any())
                {
                    action.Parameters.AddRange(parameters);
                    foreach (var pi in methodInfo.GetParameters())
                    {
                        action.MessageExtesionLevel = pi.GetCustomAttribute<ExtensionLevelAttribute>() == null
                            ? action.MessageExtesionLevel
                            : pi.GetCustomAttribute<ExtensionLevelAttribute>().PropertInjectionLevel;
                    }
                    return;
                }
            }
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null)
                        @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null)
                            @in = new InAttribute(InclutionTypes.Path);
                    }
                    
                }
                action.MessageExtesionLevel = parameterInfo.GetCustomAttribute<ExtensionLevelAttribute>() == null
                           ? action.MessageExtesionLevel
                           : parameterInfo.GetCustomAttribute<ExtensionLevelAttribute>().PropertInjectionLevel;
                action.Parameters.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
        }

        internal static List<IHeaderHandler> GetHeaderInspectors(MethodInfo methodInfo)
        {
            var inspectors = GetInspectors(methodInfo);
            var headerInspectors = GetServices<IHeaderHandler>();
            var handlers = new List<IHeaderHandler>();
            if (headerInspectors != null) handlers.AddRange(headerInspectors);
            foreach (var inspector in inspectors)
            {
                handlers.AddRange(inspector.GetHandlers());
            }
            return handlers;
        }

        private static List<IHeaderInspector> GetInspectors(MethodInfo methodInfo)
        {
            var inspectors = methodInfo.GetCustomAttributes().OfType<IHeaderInspector>().ToList();
            var typeInspectors = methodInfo.DeclaringType.GetCustomAttributes().OfType<IHeaderInspector>();
            var enumerable = typeInspectors as IHeaderInspector[] ?? typeInspectors.Where(i => inspectors.All(x => x.GetType() != i.GetType())).ToArray();
            if (enumerable.Any()) inspectors.AddRange(enumerable);
            var assemblyInstpctors = methodInfo.DeclaringType.Assembly.GetCustomAttributes().OfType<IHeaderInspector>().Where(i => inspectors.All(x => x.GetType() != i.GetType())).ToArray();
            if (assemblyInstpctors.Any()) inspectors.AddRange(assemblyInstpctors);
            return inspectors;
        }

        internal static List<HttpMethod> GetHttpMethods(List<IActionHttpMethodProvider> actions, MethodInfo method)
        {
            var methodResolver = GetService<IWebMethodConverter>();
            var methods = new List<HttpMethod>();
            if (methodResolver != null) methods.AddRange(methodResolver.GetHttpMethods(method));
            foreach (var actionHttpMethodProvider in actions)
            {
                methods.AddRange(actionHttpMethodProvider.HttpMethods);
            }
            if (methods.Count == 0) methods.Add(HttpMethod.Get);
            return methods;
        }


        internal const string ActionIdName = "sd-ActionId";

        public static void SetXmlSerializer(Type type)
        {
            xmlSerializer = type;
        }
    }
}