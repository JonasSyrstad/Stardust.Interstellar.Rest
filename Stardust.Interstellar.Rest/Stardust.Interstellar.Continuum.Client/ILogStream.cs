using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Client;

namespace Stardust.Continuum.Client
{
    [IRoutePrefix("api/v1")]
    [ApiKey]
    [CircuitBreaker(10, 1, 5)]
    [PerformanceHeaders]
    [CallingMachineName]
    [IAuthorize]
    public interface ILogStream
    {
        [HttpPut]
        [Route("single/{project}/{environment}")]
        Task AddStream([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]StreamItem item);

        [Route("batch/{project}/{environment}")]
        [HttpPut]
        Task AddStreamBatch([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]StreamItem[] items);
    }

    public static class ContinuumClient
    {
        private static string url;

        public static string BaseUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    url = ConfigurationManager.AppSettings["continuum:apiUrl"];
                }
                return url;
            }
            set { url = value; }
        }

        private static string _project;

        public static string Project
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_project))
                {
                    _project = ConfigurationManager.AppSettings["continuum:project"];
                }
                return _project;
            }
            set { _project = value; }
        }

        private static string _environment;
        private static ILogStream client;

        public static string Environment
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_environment))
                {
                    _environment = ConfigurationManager.AppSettings["continuum:environment"];
                }
                return _environment;
            }
            set { _environment = value; }
        }

        public static void SetApiKey(string apiKey)
        {
            LogStreamConfig.ApiKey = apiKey;
        }

        public static void AddStream(StreamItem item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LogStreamClient.AddStream(Project, Environment, item);
                }
                catch 
                {
                    
                }
            });
        }

        public static void AddStream(StreamItem[] items)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LogStreamClient.AddStreamBatch(Project, Environment, items);
                }
                catch
                {

                }
            });
        }

        public static void AddStream(string environment, StreamItem item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LogStreamClient.AddStream(Project, environment, item);
                }
                catch
                {

                }
            });
        }

        public static void AddStream(string environment,StreamItem[] items)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LogStreamClient.AddStreamBatch(Project, environment, items);
                }
                catch
                {

                }
            });
        }

        public static void AddStream(string project, string environment, StreamItem item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LogStreamClient.AddStream(project, environment, item);
                }
                catch
                {

                }
            });
        }

        public static void AddStream(string project, string environment, StreamItem[] items)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LogStreamClient.AddStreamBatch(project, environment, items);
                }
                catch
                {

                }
            });
        }

        private static ILogStream LogStreamClient
        {
            get
            {
                if (client == null)
                    client = ProxyFactory.CreateInstance<ILogStream>(BaseUrl);
                var c = client;
                return c;
            }
        }
    }
}