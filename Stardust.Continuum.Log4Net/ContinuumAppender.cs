using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Stardust.Continuum.Client;
using Stardust.Interstellar.Rest.Client;

namespace Stardust.Continuum.Log4Net
{
    public class BufferedHttpLogger<T> where T : class
    {
        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();
        private readonly Func<T[], Task> _transmitterAction;
        private ConcurrentBag<T> buffer = new ConcurrentBag<T>();
        public bool Running { get; private set; }
        public BufferedHttpLogger(Func<T[], Task> transmitterAction)
        {
            _transmitterAction = transmitterAction;
            Start();
        }

        public void Enqueue(T item)
        {
            _synclock.TryEnterReadLock(2);
            try
            {
                buffer.Add(item);
            }
            finally
            {
                _synclock.ExitReadLock();
            }

        }
        public void Start()
        {
            Running = true;
            Task.Run(async () =>
            {
                try
                {
                    while (Running)
                    {
                        try
                        {
                            if (buffer.IsEmpty)
                            {
                                await Task.Delay(200);
                                continue;
                            }

                            T[] buff;
                            buff = ExtractBuffer();
                            await _transmitterAction.Invoke(buff);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
            });
        }

        private T[] ExtractBuffer()
        {
            T[] buff;
            _synclock.TryEnterWriteLock(10);
            ConcurrentBag<T> b;
            try
            {
                b = buffer;
                buffer = new ConcurrentBag<T>();
            }
            finally
            {
                _synclock.ExitWriteLock();
            }
            buff = b?.ToArray();
            return buff;
        }

        public void Stop()
        {
            Running = false;
        }
    }

    public class ContinuumAppender : IBulkAppender, IAppender, IOptionHandler
    {
        private static string _correlationtoken;
        private static Func<string> _userNameFunc;
        private static BufferedHttpLogger<StreamItem> _queue;
        private ILogStream _client;


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
            _queue.Enqueue(CreateLogMessage(loggingEvent));
        }

        private StreamItem CreateLogMessage(LoggingEvent loggingEvent)
        {
            return new StreamItem
            {
                Environment = EnvironmentName,
                CorrelationToken = loggingEvent.GetCustomProperty("correlationToken"),
                UserName = _userNameFunc?.Invoke() ?? loggingEvent.Identity,
                LogLevel = loggingEvent.GetLogLevel(),
                ServiceName = ServiceName ?? (loggingEvent.GetCustomProperty("serviceName") ?? loggingEvent.Domain),
                Message = loggingEvent?.ExceptionObject?.Message ?? loggingEvent.RenderedMessage,
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
            if (_queue == null)
            {
                _client = ProxyFactory.CreateInstance<ILogStream>(ContinuumClient.BaseUrl);
                _queue = new BufferedHttpLogger<StreamItem>(async items =>
                    {
                        try
                        {
                            await _client.AddStreamBatch(ContinuumClient.Project, ContinuumClient.Environment, items);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }
                    });
            }
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


        public string ConfigLocation { get; set; }

        public string ConfigSetName { get; set; }

        public string ApiToken { get; set; }

        public string ApiTokenKey { get; set; }
    }
}
