using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Service;
using Swashbuckle.Swagger;

namespace Swashbuckle.Stardust.Interstellar
{
    public class StardustOperationDescriptor : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var implementationType = GetImplementation(apiDescription);
            if (implementationType != null)
            {
                var methods = implementationType.GetMethods().Length == 0 ? implementationType.GetInterfaces().First().GetMethods() : implementationType.GetMethods();
                var actionName = apiDescription.ActionDescriptor.ActionName;
                var impMethod = from m in methods where m.Name == actionName select m;
                var methodInfos = impMethod as MethodInfo[] ?? impMethod.ToArray();
                if (methodInfos.Count() == 1)
                {
                    var method = methodInfos.Single();
                    var methodParams = GetMethodParams(method);
                    SetDescription(operation, method,schemaRegistry);
                    var headerTypes = methodParams.Where(p => p.In == InclutionTypes.Header).ToArray();
                    foreach (var item in headerTypes)
                    {
                        operation.parameters.Add(new Parameter
                        {
                            name = item.Name,
                            @in = "header",
                            type = item.Type.Name.ToLower()
                        });
                    }
                }
                if (operation.parameters == null) operation.parameters = new List<Parameter>();
                operation.parameters.Add(new Parameter
                {
                    name = "x-supportCode",
                    description = "a support code provided by the client or generated by the service. Can be used to track requests accross processes",
                    @in = "header",
                    type = "string",
                    required = false,
                    maxItems = 1
                });
                operation.parameters.Add(new Parameter
                {
                    name = "x-stardustMeta",
                    description = "provide information regarding the calling process and its environment",
                    @in = "header",
                    type = "string",
                    required = false,
                    maxItems = 1,
                    schema = schemaRegistry.GetOrRegister(typeof(RequestHeader)),
                    format = "byte"

                });
                foreach (var response in operation.responses)
                {
                    if (response.Value.headers == null) response.Value.headers = new Dictionary<string, Header>();
                    response.Value.headers.Add("x-stardustMeta", new Header
                    {
                        description = "Contains meta information from the server and its environment",
                        format = "byte",
                        type = schemaRegistry.GetOrRegister(typeof(ResponseHeader)).@ref,
                        

                    });
                    response.Value.headers.Add("x-supportCode", new Header
                    {
                        description = "a support code provided by the client or generated by the service. Can be used to track requests accross processes",
                        type = "string"
                    });
                }
            }
        }

        private static void SetDescription(Operation operation, MethodInfo method, SchemaRegistry schemaRegistry)
        {
            var descAttrib = method.GetCustomAttribute<ServiceDescriptionAttribute>();
            if (descAttrib != null)
            {
                operation.description = descAttrib.Description;
                if (!string.IsNullOrWhiteSpace(descAttrib.Tags))
                    operation.tags = descAttrib.Tags?.Split('|');
                operation.summary = descAttrib.Summary;
                operation.deprecated = descAttrib.IsDeprecated;
                if (descAttrib.Responses != null)
                {
                    foreach (var resp in descAttrib.Responses.Split('|'))
                    {
                        if (resp.Contains(";"))
                        {
                            var respValue = resp.Split(';');
                            operation.responses.Add(respValue[0], new Response { description = respValue[1],schema =respValue.Length>2?schemaRegistry.GetOrRegister(Type.GetType(respValue[2])):null });
                        }
                    }
                }
            }
        }

        private static List<ParameterWrapper> GetMethodParams(MethodInfo implementationMethod)
        {
            var resolver = ExtensionsFactory.GetService<IServiceParameterResolver>();
            var resolvedParams = resolver?.ResolveParameters(implementationMethod);
            if (resolvedParams != null && resolvedParams.Count() == implementationMethod.GetParameters().Length) return resolvedParams.ToList();
            var methodParams = new List<ParameterWrapper>();
            foreach (var parameterInfo in implementationMethod.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null) @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null) @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                methodParams.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
            return methodParams;
        }

        private static ConcurrentDictionary<Type, Type> controllerCache = new ConcurrentDictionary<Type, Type>();
        private static IEnumerable<Type> stardustControllers;

        private static Type GetImplementation(ApiDescription apiDescription)
        {
            var controllerType = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;
            Type implementationType;
            if (controllerCache.TryGetValue(controllerType, out implementationType)) return implementationType;
            if (stardustControllers == null)
                stardustControllers = ServiceFactory.GetTypes();
            var t = stardustControllers.SingleOrDefault(c => c == controllerType);
            if (t == null)
            {
                stardustControllers = ServiceFactory.GetTypes();
                t = stardustControllers.SingleOrDefault(c => c == t);
            }
            if (t != null)
            {
                implementationType = t.BaseType.GenericTypeArguments.FirstOrDefault();
                controllerCache.TryAdd(t, implementationType);
            }

            return implementationType;
        }
    }
}