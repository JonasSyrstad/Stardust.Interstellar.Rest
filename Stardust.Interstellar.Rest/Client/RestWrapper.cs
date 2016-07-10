using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;

namespace Stardust.Interstellar.Rest.Client
{
    public class RestWrapper
    {
        private IAuthenticationHandler authenticationHandler;

        private readonly IEnumerable<IHeaderHandler> headerHandlers;

        private readonly Type interfaceType;

        private static ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>> cache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>>();

        private string baseUri;

        private readonly CookieContainer cookieContainer;

        private ErrorHandlerAttribute errorInterceptor;

        public void SetBaseUrl(string url)
        {
            baseUri = url;
        }

        public Action<Dictionary<string, object>> Extras { get; internal set; }

        protected RestWrapper(IAuthenticationHandler authenticationHandler, IHeaderHandlerFactory headerHandlers, TypeWrapper interfaceType)
        {
            this.authenticationHandler = authenticationHandler;
            this.headerHandlers = headerHandlers.GetHandlers();
            this.interfaceType = interfaceType.Type;
            InitializeClient(this.interfaceType);
            cookieContainer = new CookieContainer();
        }

        public void InitializeClient(Type interfaceType)
        {
            ConcurrentDictionary<string, ActionWrapper> wrapper;
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            var newWrapper = new ConcurrentDictionary<string, ActionWrapper>();
            var templatePrefix = interfaceType.GetCustomAttribute<IRoutePrefixAttribute>()
                ?? interfaceType.GetInterfaces().FirstOrDefault()?.GetCustomAttribute<IRoutePrefixAttribute>();
            errorInterceptor = interfaceType.GetCustomAttribute<ErrorHandlerAttribute>();
            foreach (var methodInfo in interfaceType.GetMethods().Length == 0 ? interfaceType.GetInterfaces().First().GetMethods() : interfaceType.GetMethods())
            {
                var template = methodInfo.GetCustomAttribute<RouteAttribute>();
                var actionName = GetActionName(methodInfo);
                var action = new ActionWrapper { Name = actionName, ReturnType = methodInfo.ReturnType, RouteTemplate = ExtensionsFactory.GetRouteTemplate(templatePrefix, template, methodInfo), Parameters = new List<ParameterWrapper>() };
                var actions = methodInfo.GetCustomAttributes(true).OfType<IActionHttpMethodProvider>().ToList();
                var methods = ExtensionsFactory.GetHttpMethods(actions, methodInfo);
                var handlers = ExtensionsFactory.GetHeaderInspectors(methodInfo);
                action.UseXml = methodInfo.GetCustomAttributes().OfType<UseXmlAttribute>().Any();
                action.CustomHandlers = handlers.OrderBy(i=>i.ProcessingOrder).ToList();
                action.Actions = methods;
                ExtensionsFactory.BuildParameterInfo(methodInfo, action);
                newWrapper.TryAdd(action.Name, action);
            }
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            cache.TryAdd(interfaceType, newWrapper);
        }


        protected string GetActionName(MethodInfo methodInfo)
        {
            var actionName = methodInfo.Name;
            return GetActionName(actionName);
        }

        internal static string GetActionName(string actionName)
        {
            if (actionName.EndsWith("Async")) actionName = actionName.Replace("Async", "");
            return actionName;
        }

        public ResultWrapper Execute(string name, ParameterWrapper[] parameters)
        {
            HttpWebRequest req;
            var action = CreateActionRequest(name, parameters, out req);
            ResultWrapper errorResult;
            try
            {
                var response = req.GetResponse() as HttpWebResponse;
                EnsureActionId(req, response);
                GetHeaderValues(action, response);
                var result = CreateResult(action, response);
                result.ActionId = req.ActionId();
                return result;
            }
            catch (WebException webError)
            {
                EnsureActionId(webError, req);
                GetHeaderValues(action, webError.Response as HttpWebResponse);
                errorResult = HandleWebException(webError, action);
            }
            catch (Exception ex)
            {
                errorResult = HandleGenericException(ex);
            }
            errorResult.ActionId = req.ActionId();
            return errorResult;
        }

        private void GetHeaderValues(ActionWrapper action, HttpWebResponse response)
        {
            if (response == null) return;
            if (response.Cookies != null && response.Cookies.Count > 0)
                cookieContainer.Add(response.Cookies);
            foreach (var customHandler in action.CustomHandlers)
            {
                customHandler.GetHeader(response);
            }
        }

        private static ResultWrapper HandleGenericException(Exception ex)
        {
            return new ResultWrapper { Status = HttpStatusCode.BadGateway, StatusMessage = ex.Message, Error = ex };
        }

        private static ResultWrapper HandleWebException(WebException webError, ActionWrapper action)
        {
            var resp = webError.Response as HttpWebResponse;
            if (resp != null)
            {
                var result = TryGetErrorBody(action, resp);
                return new ResultWrapper
                {
                    Status = resp.StatusCode,

                    StatusMessage = resp.StatusDescription,
                    Error = webError,
                    Value = result
                };
            }
            return new ResultWrapper { Status = HttpStatusCode.BadGateway, StatusMessage = webError.Message, Error = webError };
        }

        private static string TryGetErrorBody(ActionWrapper action, HttpWebResponse resp)
        {
            try
            {
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        private static ResultWrapper CreateResult(ActionWrapper action, HttpWebResponse response)
        {
            var type = typeof(Task).IsAssignableFrom(action.ReturnType) ? action.ReturnType.GetGenericArguments().FirstOrDefault() : action.ReturnType;
            if (type == typeof(void) || type == null)
            {
                return new ResultWrapper { Type = typeof(void), IsVoid = true, Value = null, Status = response.StatusCode, StatusMessage = response.StatusDescription, ActionId = response.ActionId() };
            }
            var result = GetResultFromResponse(action, response, type);
            return new ResultWrapper { Type = type, IsVoid = false, Value = result, Status = response.StatusCode, StatusMessage = response.StatusDescription, ActionId = response.ActionId() };
        }

        private static object GetResultFromResponse(ActionWrapper action, HttpWebResponse response, Type type)
        {
            object result;
            if (action.UseXml)
            {
                result = GetSerializer().Deserialize(response.GetResponseStream(), type);
            }
            else
            {
                using (var reader = new JsonTextReader(new StreamReader(response.GetResponseStream())))
                {
                    var serializer = new JsonSerializer();
                    result = serializer.Deserialize(reader, type);
                }
            }
            return result;
        }

        private ActionWrapper CreateActionRequest(string name, ParameterWrapper[] parameters, out HttpWebRequest req)
        {
            var action = GetAction(name);
            var path = action.RouteTemplate;
            List<string> queryStrings = new List<string>();
            foreach (var source in parameters.Where(p => p.In == InclutionTypes.Path))
            {
                if (path.Contains($"{{{source.Name}}}"))
                {
                    path = path.Replace($"{{{source.Name}}}", source.value.ToString());
                }
                else
                {
                    queryStrings.Add($"{source.Name}={source.value}");
                }
            }
            if (queryStrings.Any()) path = path + "?" + string.Join("&", queryStrings);
            req = action.UseXml ? CreateRequest(path, "application/xml") : CreateRequest(path);

            req.Headers.Add(ExtensionsFactory.ActionIdName, Guid.NewGuid().ToString());
            
            req.InitializeState();
            req.Method = action.Actions.First().ToString();
            AppendHeaders(parameters, req, action);
            AppendBody(parameters, req, action);


            if (authenticationHandler == null) authenticationHandler = ExtensionsFactory.GetService<IAuthenticationHandler>();
            if (authenticationHandler != null) authenticationHandler.Apply(req);
            return action;
        }

        private ActionWrapper GetAction(string name)
        {
            ConcurrentDictionary<string, ActionWrapper> @interface;
            if (!cache.TryGetValue(interfaceType, out @interface)) throw new InvalidOperationException("Unknown interface type");

            ActionWrapper action;
            if (!@interface.TryGetValue(GetActionName(name), out action)) throw new InvalidOperationException("Unknown method");
            return action;
        }

        private HttpWebRequest CreateRequest(string path, string contentType = "application/json")
        {
            var req = WebRequest.Create(new Uri(string.Format("{0}/{1}", baseUri, path))) as HttpWebRequest;
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.Accept = contentType;
            req.ContentType = contentType;
            req.Headers.Add("Accept-Language", "en-us");
            req.UserAgent = "stardust/1.0";
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.CookieContainer = cookieContainer;
            SetTimeoutValues(req);
            return req;
        }

        private static void SetTimeoutValues(HttpWebRequest req)
        {
            if (ClientGlobalSettings.Timeout != null) req.Timeout = ClientGlobalSettings.Timeout.Value;
            if (ClientGlobalSettings.ReadWriteTimeout != null) req.ReadWriteTimeout = ClientGlobalSettings.ReadWriteTimeout.Value;
            if (ClientGlobalSettings.ContinueTimeout != null) req.ContinueTimeout = ClientGlobalSettings.ContinueTimeout.Value;
        }

        private static void AppendBody(ParameterWrapper[] parameters, HttpWebRequest req, ActionWrapper action)
        {
            if (parameters.Any(p => p.In == InclutionTypes.Body))
            {
                if (parameters.Count(p => p.In == InclutionTypes.Body) > 1)
                {
                    SerializeBody(req, parameters.Where(p => p.In == InclutionTypes.Body).Select(p => p.value).ToList(), action);
                }
                else
                {
                    var val = parameters.Single(p => p.In == InclutionTypes.Body);
                    SerializeBody(req, val.value, action);
                }
            }
            else if (req.ContentType.Contains("xml")) req.ContentType = null;
        }

        private void AppendHeaders(ParameterWrapper[] parameters, HttpWebRequest req, ActionWrapper action)
        {
            if (headerHandlers == null) return;
            foreach (var headerHandler in headerHandlers)
            {
                headerHandler.SetHeader(req);
            }
            if (action.CustomHandlers != null)
            {
                foreach (var customHandler in action.CustomHandlers)
                {
                    customHandler.SetHeader(req);
                }
            }
            foreach (var source in parameters.Where(p => p.In == InclutionTypes.Header))
            {
                req.Headers.Add(string.Format("x-{0}", source.Name), source.value.ToString());
            }
        }

        private static void SerializeBody(WebRequest req, object val, ActionWrapper action)
        {
            if (action.UseXml) XmlBodySerializer(req, val);
            else
                JsonBodySerializer(req, val);
        }

        private static void XmlBodySerializer(WebRequest req, object val)
        {
            var xmlSerializer = GetSerializer();
            xmlSerializer.Serialize(req, val);
        }

        private static ISerializer GetSerializer()
        {
            var xmlSerializer = ExtensionsFactory.GetServices<ISerializer>().SingleOrDefault(s => string.Equals(s.SerializationType, "xml", StringComparison.InvariantCultureIgnoreCase));
            if (xmlSerializer == null) throw new IndexOutOfRangeException("Could not find serializer");
            return xmlSerializer;
        }

        private static void JsonBodySerializer(WebRequest req, object val)
        {
            var serializer = new JsonSerializer();
            using (var writer = new JsonTextWriter(new StreamWriter(req.GetRequestStream())))
            {
                serializer.Serialize(writer, val);
            }
        }

        public async Task<ResultWrapper> ExecuteAsync(string name, ParameterWrapper[] parameters)
        {
            HttpWebRequest req;
            var action = CreateActionRequest(name, parameters, out req);
            ResultWrapper errorResult;
            try
            {
                var response = await req.GetResponseAsync() as HttpWebResponse;
                EnsureActionId(req, response);
                GetHeaderValues(action, response);
                var result = CreateResult(action, response);
                result.ActionId = req.ActionId();
                return result;
            }
            catch (WebException webError)
            {
                EnsureActionId(webError, req);
                GetHeaderValues(action, webError.Response as HttpWebResponse);
                errorResult = HandleWebException(webError, action);
            }
            catch (Exception ex)
            {
                errorResult = HandleGenericException(ex);
            }
            errorResult.ActionId = req.ActionId();
            return errorResult;
        }

        private static void EnsureActionId(WebException webError, HttpWebRequest req)
        {
            var resp = webError.Response;
            EnsureActionId(req, resp);
        }

        private static void EnsureActionId(HttpWebRequest req, WebResponse resp)
        {
            if (string.IsNullOrWhiteSpace(resp?.Headers?.Get("sd-ActionId")))
            {
                resp?.Headers?.Add("sd-ActionId", req.ActionId());
            }
        }

        public T Invoke<T>(string name, ParameterWrapper[] parameters)
        {
            var result = Execute(name, parameters);
            Extras?.Invoke(result.GetState().Extras);
            result.EndState();

            if (result.Error == null)
                return (T)result.Value;
            CreateException(name, result);
            return default(T);
        }

        public async Task<T> InvokeAsync<T>(string name, ParameterWrapper[] parameters)
        {
            var result = await ExecuteAsync(GetActionName(name), parameters);
            Extras?.Invoke(result.GetState().Extras);
            StateHelper.EndState(result.ActionId);
            if (typeof(T) == typeof(void)) return default(T);
            if (result.Error == null)
                return (T)result.Value;
            CreateException(name, result);
            return default(T);
        }

        private void CreateException(string name, ResultWrapper result)
        {
            var action = GetAction(name);
            var handler = GetErrorHandler();
            if (handler != null) throw handler.ProduceClientException(result.StatusMessage, result.Status, result.Error, result.Value as string);
            if (result.Value != null) throw new RestWrapperException(result.StatusMessage, result.Status, result.Value, result.Error);
            throw new RestWrapperException(result.StatusMessage, result.Status, result.Error);
        }

        private IErrorHandler GetErrorHandler()
        {
            IErrorHandler handler;
            var globalErrorHandler = ExtensionsFactory.GetService<IErrorHandler>();
            var errorHandler = errorInterceptor?.ErrorHandler;
            if (globalErrorHandler != null && errorHandler != null)
            {
                handler = new AggregateHandler(globalErrorHandler, errorHandler);
            }
            else if (globalErrorHandler != null)
            {
                handler = globalErrorHandler;
            }
            else
            {
                handler = errorHandler;
            }
            return handler;
        }

        public async Task InvokeVoidAsync(string name, ParameterWrapper[] parameters)
        {
            var result = await ExecuteAsync(GetActionName(name), parameters);
            if (result.Error == null)
                return;
            CreateException(name, result);
        }

        public void InvokeVoid(string name, ParameterWrapper[] parameters)
        {
            var result = Execute(name, parameters);
            if (result.Error == null)
                return;
            CreateException(name, result);
        }

        protected ParameterWrapper[] GetParameters(string name, params object[] parameters)
        {
            ConcurrentDictionary<string, ActionWrapper> item;
            if (!cache.TryGetValue(interfaceType, out item)) throw new InvalidOperationException("Invalid interface type");
            ActionWrapper action;
            if (!item.TryGetValue(GetActionName(name), out action)) throw new InvalidOperationException("Invalid action");
            var i = 0;
            var wrappers = new List<ParameterWrapper>();
            foreach (var parameter in parameters)
            {
                var def = action.Parameters[i];
                wrappers.Add(def.Create(parameter));
                i++;
            }
            return wrappers.ToArray();
        }
    }
}
