using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Continuum.Client
{
    [IRoutePrefix("api/v1")]
    [ApiKey]
    [CircuitBreaker(10,1,5)]
    [PerformanceHeaders]
    [IAuthorize]
    public interface ILogStreamClient
    {
        [HttpPut]
        [Route("single/{project}/{environment}")]
        Task AddStream([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]StreamItem item);

        [Route("batch/{project}/{environment}")]
        [HttpPut]
        Task AddStreamBatch([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]StreamItem[] items);
    }

    public class ApiKeyAttribute : AuthenticationInspectorAttributeBase, IAuthenticationHandler
    {
        public override IAuthenticationHandler GetHandler()
        {
            return this;
        }

        public void Apply(HttpWebRequest req)
        {
            req.Headers.Add("Authorization", "ApiKey " + LogStreamConfig.ApiKey);
        }
    }

    public static class LogStreamConfig
    {
        private static string _apiKey;

        public static string ApiKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_apiKey)) _apiKey = ConfigurationManager.AppSettings["continuum:apiKey"];
                return _apiKey;
            }
            set { _apiKey = value; }
        }
    }

    public class StreamItem
    {
        /// <summary>
        /// This is set by the service if not provided
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTime? Timestamp { get; set; }

        public string UserName { get; set; }

        public string CorrelationToken { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public LogLevels LogLevel { get; set; }

        public string ServiceName { get; set; }

        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the properties., not in use in the current demo....
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public Dictionary<string,object> Properties { get; set; }
    }

    public enum LogLevels
    {
        Information,
        Debug,
        Warning,
        Error,
    }
}
