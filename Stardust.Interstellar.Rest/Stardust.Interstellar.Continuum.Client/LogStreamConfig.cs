using System.Configuration;
using Stardust.Interstellar.Rest.Client;

namespace Stardust.Continuum.Client
{
    public static class LogStreamConfig
    {
        static LogStreamConfig()
        {
            ProxyFactory.CreateProxy<ILogStream>();
        }
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
}