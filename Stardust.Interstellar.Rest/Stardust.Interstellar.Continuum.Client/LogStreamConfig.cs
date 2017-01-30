using System.Configuration;

namespace Stardust.Continuum.Client
{
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
}