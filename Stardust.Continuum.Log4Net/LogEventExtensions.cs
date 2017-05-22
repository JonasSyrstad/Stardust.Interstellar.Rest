using log4net.Core;
using Stardust.Continuum.Client;

namespace Stardust.Continuum.Log4Net
{
    public static class LogEventExtensions
    {
        public static string GetCustomProperty(this LoggingEvent loggingEvent, string propertyName)
        {
            return loggingEvent.Properties.Contains(propertyName)
                ? loggingEvent.Properties[propertyName].ToString()
                : null;
        }

        public static LogLevels GetLogLevel(this LoggingEvent loggingEvent)
        {
            switch (loggingEvent?.Level?.Value)
            {
                case 50000:
                case 10000:
                case 30000:
                case 20000:
                    return LogLevels.Debug;
                case 60000:
                    return LogLevels.Warning;
                case 70000:
                case 80000:
                case 90000:
                case 110000:
                case 120000:
                    return LogLevels.Error;
                case 40000:
                default:
                    return LogLevels.Information;
            }
        }
    }
}