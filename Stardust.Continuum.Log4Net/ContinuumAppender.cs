using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Stardust.Continuum.Client;
using Stardust.Interstellar.Rest.Client;

namespace Stardust.Continuum.Log4Net
{


    public class ContinuumAppender : IBulkAppender, IAppender, IOptionHandler
    {
        private static string _correlationtoken;
        private static Func<string> _userNameFunc;

        static ContinuumAppender()
        {
            var client = ProxyFactory.CreateInstance<ILogStream>("http://continuum.stardustfx.com/dummy/api");
        }
        public void Close()
        {
            ContinuumClient.Shutdown();
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            ContinuumClient.AddStream(CreateLogMessage(loggingEvent));
        }

        private StreamItem CreateLogMessage(LoggingEvent loggingEvent)
        {
            return new StreamItem
            {
                Environment = EnvironmentName,
                CorrelationToken = loggingEvent.GetCustomProperty("correlationToken"),
                UserName = _userNameFunc?.Invoke()??loggingEvent.Identity,
                LogLevel = loggingEvent.GetLogLevel(),
                ServiceName = ServiceName ?? (loggingEvent.GetCustomProperty("serviceName") ?? loggingEvent.Domain),
                Message = loggingEvent?.ExceptionObject?.StackTrace ?? loggingEvent.RenderedMessage,
                StackTrace = loggingEvent?.ExceptionObject?.StackTrace,
                Timestamp = loggingEvent.TimeStamp,
                Properties = Get(loggingEvent)
            };
        }

        private static Dictionary<string, object> Get(LoggingEvent loggingEvent)
        {
            var props = new Dictionary<string, object>
            {
                {"machineName",Environment.MachineName},

            };
            foreach (var loggingEventProperty in loggingEvent.Properties.GetKeys())
            {
                props.Add(loggingEventProperty, loggingEvent.GetCustomProperty(loggingEventProperty));
            }
            return props;
        }

        public string ServiceName { get; set; }


        public string EnvironmentName { get; set; }

        public string Name { get; set; }
        public void DoAppend(LoggingEvent[] loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
            {
                DoAppend(loggingEvent);
            }
        }

        public void ActivateOptions()
        {
            ContinuumClient.Environment = EnvironmentName;
            ContinuumClient.Project = ProjectName;
            ContinuumClient.SetApiKey(ApiKey);
            ContinuumClient.LimitMessageSize = MaxMessageSize;
            ContinuumClient.BaseUrl = StreamUrl;
        }

        public int? MaxMessageSize { get; set; }

        public string ApiKey { get; set; }

        public bool BatchAll { get; set; }

        public string ProjectName { get; set; }

        public string StreamUrl { get; set; }

        public static void SetUserNameResolver(Func<string> getUserName)
        {
            _userNameFunc = getUserName;
        }
    }
}
