using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Stardust.Interstellar.Rest.Client;

namespace Stardust.Continuum.Client
{
    public static class ContinuumClient
    {
        /// <summary>
        /// If not null and greater than 1000 characters the streamed message element will be truncated for buffered transmissions
        /// </summary>
        /// <value>
        /// The size of the limit message.
        /// </value>
        public static int? LimitMessageSize { get; set; }
        private static Timer _timer = new Timer() { Interval = 500, AutoReset = true, Enabled = true };
        private static string url;

        private static ConcurrentBag<StreamItem> _logBuffer = new ConcurrentBag<StreamItem>();

        private static object triowing = new object();

        static ContinuumClient()
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        public static void Shutdown()
        {
            _timer.Enabled = false;
            _logPumpIsDisabled = true;
            Flush();
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            try
            {
                Flush();
            }
            finally
            {
                _timer.Enabled = true;
            }
        }

        private static void Flush()
        {
            StreamItem[] items;
            lock (triowing)
            {
                ConcurrentBag<StreamItem> tempBuffer;
                lock (bufferLock)
                {
                    tempBuffer = _logBuffer;
                    _logBuffer = new ConcurrentBag<StreamItem>();
                }
                items = tempBuffer.ToArray();
            }
            switch (items.Length)
            {
                case 0:
                    return;
                case 1:
                    Task.Run(async () => await AddStreamInternal(items[0])).Wait();
                    break;
                default:
                    Task.Run(async () => await AddStreamInternal(items.Reverse().ToArray())).Wait();
                    break;
            }
        }

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
        private static object bufferLock = new object();
        private static bool _logPumpIsDisabled;

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

        public static void AddStream(StreamItem item, bool buffered = true)
        {

            if (buffered)
            {
                if(_logPumpIsDisabled) throw new ObjectDisposedException(nameof(ContinuumClient),"Batch message pump is closed");
                if (LimitMessageSize.HasValue && LimitMessageSize.Value > 1000)
                    item.Message = item.Message?.Substring(0, LimitMessageSize.Value);
                lock (bufferLock)
                {
                    _logBuffer.Add(item);
                }

            }
            else
            {
                Task.Run(async () =>
                {
                    await AddStreamInternal(item);
                });
            }
        }

        private static async Task AddStreamInternal(StreamItem item)
        {
            try
            {
                await LogStreamClient.AddStream(Project, Environment, item);
            }
            catch
            {
            }
        }

        public static void AddStream(StreamItem[] items)
        {
            Task.Run(async () =>
            {
                await AddStreamInternal(items);
            });
        }

        private static async Task AddStreamInternal(StreamItem[] items)
        {
            try
            {
                await LogStreamClient.AddStreamBatch(Project, Environment, items);
            }
            catch
            {
            }
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

        public static void AddStream(string environment, StreamItem[] items)
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