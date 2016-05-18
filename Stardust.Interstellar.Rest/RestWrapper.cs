using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest
{
    public class RestWrapper
    {
        internal const string ActionIdName = "sd-ActionId";

        private readonly IAuthenticationHandler authenticationHandler;

        private readonly IEnumerable<IHeaderHandler> headerHandlers;

        private readonly Type interfaceType;

        private static ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>> cache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>>();

        private string baseUri;

        public void SetBaseUrl(string url)
        {
            baseUri = url;
        }

        protected RestWrapper(IAuthenticationHandler authenticationHandler, IHeaderHandlerFactory headerHandlers, TypeWrapper interfaceType)
        {
            this.authenticationHandler = authenticationHandler;
            this.headerHandlers = headerHandlers.GetHandlers();
            this.interfaceType = interfaceType.Type;
            InitializeClient(this.interfaceType);
        }

        public void InitializeClient(Type interfaceType)
        {
            ConcurrentDictionary<string, ActionWrapper> wrapper;
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            var newWrapper = new ConcurrentDictionary<string, ActionWrapper>();
            var templatePrefix = interfaceType.GetCustomAttribute<IRoutePrefixAttribute>();
            foreach (var methodInfo in interfaceType.GetMethods())
            {
                var template = methodInfo.GetCustomAttribute<RouteAttribute>();
                var actionName = GetActionName(methodInfo);
                var action = new ActionWrapper { Name = actionName, ReturnType = methodInfo.ReturnType, RouteTemplate = ExtensionsFactory.GetRouteTemplate(templatePrefix, template,methodInfo), Parameters = new List<ParameterWrapper>() };
                var actions = methodInfo.GetCustomAttributes(true).OfType<IActionHttpMethodProvider>().ToList();
                var methods = ExtensionsFactory.GetHttpMethods(actions,methodInfo);
                var handlers = ExtensionsFactory.GetHeaderInspectors(methodInfo);
                action.CustomHandlers = handlers;
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
                return CreateResult(action, response);
            }
            catch (WebException webError)
            {
                errorResult = HandleWebException(webError, action);
            }
            catch (Exception ex)
            {
                errorResult= HandleGenericException(ex);
            }
            errorResult.ActionId = req.ActionId();
            return errorResult;
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

        private static object TryGetErrorBody(ActionWrapper action, HttpWebResponse resp)
        {
            object result = null;
            try
            {
                using (var reader = new JsonTextReader(new StreamReader(resp.GetResponseStream())))
                {
                    var serializer = new JsonSerializer();
                    result = serializer.Deserialize(reader, action.ReturnType);
                }
            }
            catch
            {
                // ignored
            }
            return result;
        }

        private static ResultWrapper CreateResult(ActionWrapper action, HttpWebResponse response)
        {
            var type = typeof(Task).IsAssignableFrom(action.ReturnType) ? action.ReturnType.GetGenericArguments().First() : action.ReturnType;
            if (type == typeof(void))
            {
                return new ResultWrapper { Type = typeof(void), IsVoid = true, Value = null, Status = response.StatusCode, StatusMessage = response.StatusDescription,ActionId = response.ActionId()};
            }
            using (var reader = new JsonTextReader(new StreamReader(response.GetResponseStream())))
            {

                var serializer = new JsonSerializer();
                var result = serializer.Deserialize(reader, type);
                return new ResultWrapper { Type = type, IsVoid = false, Value = result, Status = response.StatusCode, StatusMessage = response.StatusDescription, ActionId = response.ActionId() };
            }
        }

        private ActionWrapper CreateActionRequest(string name, ParameterWrapper[] parameters, out HttpWebRequest req)
        {
            ConcurrentDictionary<string, ActionWrapper> @interface;
            if (!cache.TryGetValue(interfaceType, out @interface)) throw new InvalidOperationException("Unknown interface type");

            ActionWrapper action;
            if (!@interface.TryGetValue(GetActionName(name), out action)) throw new InvalidOperationException("Unknown method");
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
            req = CreateRequest(path);
            StatefullHeaderHandlerBase.InitializeState(req);
            req.Headers.Add(ActionIdName,Guid.NewGuid().ToString());
            req.Method = action.Actions.First().ToString();
            AppendHeaders(parameters, req, action);
            AppendBody(parameters, req);
            if (authenticationHandler != null) authenticationHandler.Apply(req);
            return action;
        }

        


        private HttpWebRequest CreateRequest(string path)
        {
            var req = WebRequest.Create(new Uri(string.Format("{0}/{1}", baseUri, path))) as HttpWebRequest;
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.Accept = "application/json";
            req.ContentType = "application/json";
            req.Headers.Add("Accept-Language", "en-us");
            req.UserAgent = "stardust/1.0";
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return req;
        }

        private static void AppendBody(ParameterWrapper[] parameters, HttpWebRequest req)
        {
            if (parameters.Any(p => p.In == InclutionTypes.Body))
            {
                if (parameters.Count(p => p.In == InclutionTypes.Body) > 1)
                {
                    SerializeBody(req, parameters.Where(p => p.In == InclutionTypes.Body).Select(p => p.value).ToList());
                }
                else
                {
                    var val = parameters.Single(p => p.In == InclutionTypes.Body);
                    SerializeBody(req, val.value);
                }
            }
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

        private static void SerializeBody(WebRequest req, object val)
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
                return CreateResult(action, response);
            }
            catch (WebException webError)
            {
                errorResult= HandleWebException(webError, action);
            }
            catch (Exception ex)
            {
                errorResult= HandleGenericException(ex);
            }
            errorResult.ActionId = req.ActionId();
            return errorResult;  
        }

        public T Invoke<T>(string name, ParameterWrapper[] parameters)
        {
            var result = Execute(name, parameters);
            StatefullHeaderHandlerBase.EndState(result);
            if (result.Error == null)
                return (T)result.Value;
            if (result.Value != null)
                throw new RestWrapperException<T>(result.StatusMessage, result.Status, result.Value, result.Error);
            throw new RestWrapperException(result.StatusMessage, result.Status, result.Error);
        }

        public async Task<T> InvokeAsync<T>(string name, ParameterWrapper[] parameters)
        {
            var result = await ExecuteAsync(GetActionName(name), parameters);
            if (typeof(T) == typeof(void)) return default(T);
            if (result.Error == null)
                return (T)result.Value;
            if (result.Value != null)
                throw new RestWrapperException<T>(result.StatusMessage, result.Status, result.Value, result.Error);
            throw new RestWrapperException(result.StatusMessage, result.Status, result.Error);
        }

        public async Task InvokeVoidAsync(string name, ParameterWrapper[] parameters)
        {
            var result = await ExecuteAsync(GetActionName(name), parameters);
            if (result.Error == null)
                return;
            throw new RestWrapperException(result.StatusMessage, result.Status, result.Error);
        }

        public void InvokeVoid(string name, ParameterWrapper[] parameters)
        {
            var result = Execute(name, parameters);
            if (result.Error == null)
                return;
            throw new RestWrapperException(result.StatusMessage, result.Status, result.Error);
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
