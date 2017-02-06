using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Stardust.Particles;

namespace Stardust.Continuum
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Filters.Add(new ApiKeyAuthenticationFilter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }

    public class ApiKeyAuthenticationFilter : IAuthenticationFilter
    {
        private static ConcurrentDictionary<string, List<string>> apiKeys = new ConcurrentDictionary<string, List<string>>();
        private static ConcurrentDictionary<string, bool> apiKeyState = new ConcurrentDictionary<string, bool>();
        public bool AllowMultiple { get { return false; } }
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var configKey = string.Join(".", context.ActionContext.ControllerContext.RouteData.Values.Values);
            try
            {
                
                if (!context.Request.Headers.Authorization.Scheme.Equals("ApiKey", StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrWhiteSpace(context.Request.Headers.Authorization.Parameter)&&context.Request.RequestUri.ParseQueryString()["api_key"].IsNullOrWhiteSpace())
                    Logging.DebugMessage( $"No api-key provided for {configKey}");
                var apiKey = context.Request.Headers.Authorization.Parameter;
                if (apiKey.IsNullOrWhiteSpace())
                    apiKey = context.Request.RequestUri.ParseQueryString()["api_key"];
                bool isAllowed;
                
                if (!apiKeyState.TryGetValue($"{apiKey}.{configKey}", out isAllowed))
                {
                    List<string> setting;
                    if (!apiKeys.TryGetValue(configKey, out setting))
                    {
                        setting = ConfigurationManager.AppSettings[configKey]?.Split('|').ToList();
                        apiKeys.TryAdd(configKey, setting);
                    }

                    isAllowed = setting.Contains(apiKey);
                    apiKeyState.TryAdd(apiKey, isAllowed);
                }
                if (!isAllowed)
                {
                    Logging.DebugMessage($"Invalid api key for {configKey}");
                    context.ErrorResult = new UnauthorizedResult(new List<AuthenticationHeaderValue>(), context.Request);
                    return Task.FromResult(0);
                }
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Logging.DebugMessage($"Invalid api key for {configKey}: {ex.Message}");
                context.ErrorResult = new UnauthorizedResult(new List<AuthenticationHeaderValue>(), context.Request);
                return Task.FromResult(0);
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
