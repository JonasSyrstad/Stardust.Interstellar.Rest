using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Service
{
    public abstract class ServiceWrapperBase<T> : ApiController
    {
        private const string ActionWrapperName = "sd-actionWrapperName";

        protected readonly T implementation;

        protected HttpResponseMessage CreateResponse<TMessage>(HttpStatusCode statusCode, TMessage message = default(TMessage))
        {
            HttpResponseMessage result;

            if (message == null)
            {
                result = Request.CreateResponse(HttpStatusCode.NoContent);
            }
            else
                result = Request.CreateResponse(statusCode, message);
            SetHeaders(result);
            Request.EndState();

            return result;
        }

        private void SetHeaders(HttpResponseMessage result)
        {
            var action = GetActionWrapper(Request.Headers.Where(h => h.Key == ActionWrapperName).Select(h => h.Value).FirstOrDefault().FirstOrDefault());
            var actionId = Request.ActionId();
            result.Headers.Add(ExtensionsFactory.ActionIdName, actionId);
            foreach (var customHandler in action.CustomHandlers)
            {
                customHandler.SetServiceHeaders(result.Headers);
            }
        }

        protected async Task<HttpResponseMessage> CreateResponseAsync<TMessage>(HttpStatusCode statusCode, Task<TMessage> messageTask = null)
        {
            HttpResponseMessage result;
            try
            {
                var message = await messageTask;
                if (message == null) result = Request.CreateResponse(HttpStatusCode.NoContent);
                else result = Request.CreateResponse(statusCode, message);
                SetHeaders(result);
                Request.EndState();
                return result;
            }
            catch (Exception ex)
            {
                result = CreateErrorResponse(ex);
                return result;
            }
        }

        protected async Task<HttpResponseMessage> CreateResponseVoidAsync(HttpStatusCode statusCode, Task messageTask)
        {
            HttpResponseMessage result;
            try
            {
                await messageTask;
                result = Request.CreateResponse(HttpStatusCode.NoContent);
                SetHeaders(result);
                Request.EndState();
            }
            catch (Exception ex)
            {
                result = CreateErrorResponse(ex);
            }
            return result;
        }

        protected HttpResponseMessage CreateErrorResponse(Exception ex)
        {
            var errorHandler = GetErrorHandler();
            HttpResponseMessage result = null;
            result = ConvertExceptionToErrorResult(ex, result, errorHandler);
            if (result == null) result = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            SetHeaders(result);
            Request.EndState();
            return result;
        }

        private IErrorHandler GetErrorHandler()
        {

            var errorHandler = ExtensionsFactory.GetService<IErrorHandler>();
            if (errorHandler == null) return errorInterceptor;
            if (errorInterceptor != null) return new AggregateHandler(errorHandler, errorInterceptor);

            return errorHandler;
        }

        private HttpResponseMessage ConvertExceptionToErrorResult(Exception ex, HttpResponseMessage result, IErrorHandler errorHandler)
        {
            if (errorHandler != null && errorHandler.OverrideDefaults)
            {
                result = errorHandler.ConvertToErrorResponse(ex, Request);

                if (result != null) return result;
            }
            if (ex is UnauthorizedAccessException) result = Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message);
            else if (ex is IndexOutOfRangeException || ex is KeyNotFoundException) result = Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            else if (ex is NotImplementedException) result = Request.CreateErrorResponse(HttpStatusCode.NotImplemented, ex.Message);
            else if (errorHandler != null) result = errorHandler.ConvertToErrorResponse(ex, Request);
            return result;
        }

        private static ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>> cache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>>();

        private IErrorHandler errorInterceptor;

        protected ServiceWrapperBase(T implementation)
        {
            this.implementation = implementation;
            InitializeServiceApi(typeof(T));
        }

        protected ParameterWrapper[] GatherParameters(string name, object[] fromWebMethodParameters)
        {
            try
            {

                Request.InitializeState();
                var action = GetAction(name);
                Request.GetState().SetState("controller", this);
                Request.GetState().SetState("controllerName", typeof(T).FullName);
                Request.GetState().SetState("action", action.Name);
                var i = 0;
                var wrappers = new List<ParameterWrapper>();
                foreach (var parameter in action.Parameters)
                {
                    var val = parameter.In == InclutionTypes.Header ? GetFromHeaders(parameter) : fromWebMethodParameters[i];
                    wrappers.Add(parameter.Create(val));
                    i++;
                }
                this.Request.Headers.Add(ActionWrapperName, GetActionName(name));
                foreach (var headerHandler in action.CustomHandlers)
                {
                    headerHandler.GetServiceHeader(Request.Headers);
                }
                return wrappers.ToArray();
            }
            catch (Exception ex)
            {

                throw new ParameterReflectionException(string.Format("Unable to gather parameters for {0}", name), ex);
            }

        }

        private static ActionWrapper GetAction(string name)
        {
            var actionName = GetActionName(name);
            return GetActionWrapper(actionName);
        }

        private static ActionWrapper GetActionWrapper(string actionName)
        {
            ConcurrentDictionary<string, ActionWrapper> item;
            if (!cache.TryGetValue(typeof(T), out item)) throw new InvalidOperationException("Invalid interface type");
            ActionWrapper action;
            if (!item.TryGetValue(actionName, out action)) throw new InvalidOperationException("Invalid action");
            return action;
        }

        private object GetFromHeaders(ParameterWrapper parameter)
        {
            if (!Request.Headers.Contains(parameter.Name)) return null;
            var vals = Request.Headers.GetValues(parameter.Name);
            if (vals.Count() <= 1) return vals.SingleOrDefault();
            throw new InvalidCastException("Not currently able to deal with collections...");
        }

        private static ConcurrentDictionary<Type, ErrorHandlerAttribute> errorhanderCache = new ConcurrentDictionary<Type, ErrorHandlerAttribute>();
        protected void InitializeServiceApi(Type interfaceType)
        {
            GetErrorInterceptor(interfaceType);
            ConcurrentDictionary<string, ActionWrapper> wrapper;
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            var newWrapper = new ConcurrentDictionary<string, ActionWrapper>();

            foreach (var methodInfo in interfaceType.GetMethods().Length == 0 ? interfaceType.GetInterfaces().First().GetMethods() : interfaceType.GetMethods())
            {
                var template = ExtensionsFactory.GetServiceTemplate(methodInfo);
                var actionName = GetActionName(methodInfo);
                var action = new ActionWrapper { Name = actionName, ReturnType = methodInfo.ReturnType, RouteTemplate = template, Parameters = new List<ParameterWrapper>() };
                var actions = methodInfo.GetCustomAttributes(true).OfType<IActionHttpMethodProvider>();
                var methods = ExtensionsFactory.GetHttpMethods(actions.ToList(), methodInfo);
                var handlers = ExtensionsFactory.GetHeaderInspectors(methodInfo);
                action.CustomHandlers = handlers;
                action.Actions = methods;
                BuildParameterInfo(methodInfo, action);
                newWrapper.TryAdd(action.Name, action);
            }
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            cache.TryAdd(interfaceType, newWrapper);
        }

        private void GetErrorInterceptor(Type interfaceType)
        {
            ErrorHandlerAttribute errorHanderInterceptor;
            if (!errorhanderCache.TryGetValue(interfaceType, out errorHanderInterceptor))
            {
                errorHanderInterceptor = interfaceType.GetCustomAttribute<ErrorHandlerAttribute>();
                errorhanderCache.TryAdd(interfaceType, errorHanderInterceptor);
            }
            if (errorHanderInterceptor != null) errorInterceptor = errorHanderInterceptor.ErrorHandler;
        }

        private static List<IHeaderHandler> GetHeaderInspectors(MethodInfo methodInfo)
        {
            var inspectors = methodInfo.GetCustomAttributes().OfType<IHeaderInspector>();
            var handlers = new List<IHeaderHandler>();
            foreach (var inspector in inspectors)
            {
                handlers.AddRange(inspector.GetHandlers());
            }
            return handlers;
        }

        private static List<HttpMethod> GetHttpMethods(IEnumerable<IActionHttpMethodProvider> actions)
        {
            var methods = new List<HttpMethod>();
            foreach (var actionHttpMethodProvider in actions)
            {
                methods.AddRange(actionHttpMethodProvider.HttpMethods);
            }
            if (methods.Count == 0) methods.Add(HttpMethod.Get);
            return methods;
        }

        protected string GetActionName(MethodInfo methodInfo)
        {
            var actionName = methodInfo.Name;
            return GetActionName(actionName);
        }

        private static string GetActionName(string actionName)
        {
            if (actionName.EndsWith("Async")) actionName = actionName.Replace("Async", "");
            return actionName;
        }

        private static void BuildParameterInfo(MethodInfo methodInfo, ActionWrapper action)
        {
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
                action.Parameters.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
        }
    }
}
