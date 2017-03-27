using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Owin;
using Stardust.Continuum.Client;
using Stardust.Continuum.Controllers;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using WebGrease.Configuration;
using Timer = System.Timers.Timer;

[assembly: OwinStartup(typeof(Stardust.Continuum.Startup))]

namespace Stardust.Continuum
{
    public partial class Startup
    {
        public static IHubContext hub;

        public void Configuration(IAppBuilder app)
        {

            ConfigureAuth(app);
            app.MapSignalR("/signalr", new HubConfiguration
            {
                EnableJSONP = true,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true,

            });
            if (!ConfigurationManagerHelper.GetValueOnKey("authority").IsNullOrWhiteSpace())
                GlobalHost.HubPipeline.RequireAuthentication();
            hub = GlobalHost.ConnectionManager.GetHubContext<StreamHub>();


        }
    }

    [HubName("updateFeed")]
    public class StreamHub : Hub
    {
        [HubMethodName("cmessage")]
        public void SendMessage(string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<StreamHub>();

            context.Clients.All.cmessage(message);
        }

        [HubMethodName("ping")]
        public void Ping(string clientId) { }

        [HubMethodName("connect")]
        public async Task Connect(string message, string subcriptionName)
        {
            foreach (var @group in HomeController.itemd.Where(i => i != "-Select source-"))
            {
                try
                {
                    await Groups.Remove(Context.ConnectionId, @group);

                }
                catch (Exception)
                {

                }
            }
            Clients.Caller.cmessage(new StreamItem
            {
                Timestamp = DateTime.UtcNow,
                Message = $"Cleared all feed subscriptions",
                LogLevel = LogLevels.Information,
                CorrelationToken = "startup",
                UserName = "streamClient",
                ServiceName = "LogStream",
            });
            await Groups.Add(Context.ConnectionId, subcriptionName);
            Clients.Caller.cmessage(new StreamItem
            {
                Timestamp = DateTime.UtcNow,
                Message = $"{message}: {subcriptionName}",
                LogLevel = LogLevels.Information,
                CorrelationToken = "startup",
                UserName = "streamClient",
                ServiceName = "LogStream",
            });
        }

    }

    public class StreamServiceImp : ILogStream
    {
        private static long receivedTotal = 0;
        private static long receivedLastHour = 0;
        private static DateTime resetTime = DateTime.UtcNow.AddHours(1);
        private const string Sourceaddress = "SourceAddress";
        private const string Machinename = "MachineName";
        private const string XCallingmachine = "x-callingMachine";
        private static long cnt = 0;
        private static  long totalCnt = 0;
        private static long dbg = 0;
        private static long error = 0;
        private static DateTime collectionSince = DateTime.UtcNow;
        private Dictionary<string, string> _headers;
        internal static UsageItem Total = new UsageItem { Name = "Total", Location = "All", ItemsReceived = 0 };
        internal static UsageItem Errors = new UsageItem { Name = "Total", Location = "Errors", ItemsReceived = 0 };
        internal static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, UsageItem>> ActivityPrLocation = new ConcurrentDictionary<string, ConcurrentDictionary<string, UsageItem>>();
        public Task AddStream(string project, string environment, StreamItem item)
        {
            LogBytesReceived();
            var key = $"{project}.{environment}";
            try
            {
                AddCommonProperties(item);
                Interlocked.Increment(ref totalCnt);//++
                Interlocked.Increment(ref cnt);//++;
                if (item.LogLevel != LogLevels.Error)
                    Interlocked.Increment(ref dbg); //++;
                else
                    Interlocked.Increment(ref error);//++;
            }
            catch 
            {
                
            }
            try
            {
                item.Environment = environment;
                AddSource(project, environment);
                if (item.Timestamp == null) item.Timestamp = DateTime.UtcNow;
                Startup.hub.Clients.Groups(new List<string> { "All", key }).cmessage(item);
                LogUsage(item, key);
                return Task.FromResult("");
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private static void LogBytesReceived()
        {
            try
            {
                var size = HttpContext.Current.Request.ContentLength;
                Interlocked.Add(ref receivedTotal, size);
                if (resetTime < DateTime.UtcNow)
                {
                    resetTime = DateTime.UtcNow.AddHours(1);
                    Interlocked.Exchange(ref receivedLastHour , 0);
                }
                Interlocked.Add(ref receivedLastHour, size);
                //receivedLastHour += size;
            }
            catch 
            {
                
            }
        }

        private static void LogUsage(StreamItem item, string key)
        {
            try
            {
                ConcurrentDictionary<string, UsageItem> site;
                if (!ActivityPrLocation.TryGetValue(key, out site))
                {
                    site = new ConcurrentDictionary<string, UsageItem>();
                    ActivityPrLocation.TryAdd(key, site);
                }
                var loc = $"{item.ServiceName}.{item.Properties["SourceAddress"]}";
                UsageItem locationCounter;
                if (!site.TryGetValue(loc, out locationCounter))
                {
                    locationCounter = new UsageItem() { Name = key, Location = loc };
                    site.TryAdd(loc, locationCounter);
                }
                locationCounter.Increment();//.ItemsReceived++;
                Total.Increment();
                if (item.LogLevel == LogLevels.Error)
                    Errors.Increment();
            }
            catch (Exception)
            {

            }
        }

        private static void AddCommonProperties(StreamItem item)
        {
            if (item.Properties == null)
                item.Properties = new Dictionary<string, object>();
            if (!item.Properties.ContainsKey(Machinename))
                item.Properties.Add(Machinename, HttpContext.Current.Request.Headers[XCallingmachine]);
            if (!item.Properties.ContainsKey(Sourceaddress))
                item.Properties.Add(Sourceaddress, HttpContext.Current.Request.UserHostAddress);

        }

        private static void AddSource(string project, string environment)
        {
            try
            {
                if (Interlocked.Read(ref cnt) > 200)
                {

                    var errorPercent = Interlocked.Read(ref totalCnt) > 0 ? (Interlocked.Read(ref error) / Interlocked.Read(ref totalCnt)) * 100 : 0;
                    Interlocked.Exchange(ref cnt, 0);
                    var timeSpan = (DateTime.UtcNow - collectionSince);
                    Logging.DebugMessage($"{DateTime.UtcNow}: {Interlocked.Read(ref dbg):N0} debug, {Interlocked.Read(ref error):N0} error and total {Interlocked.Read(ref totalCnt):N0}. {(Interlocked.Read(ref totalCnt) / timeSpan.TotalSeconds):N2} msg/sec, {(Interlocked.Read(ref totalCnt) / timeSpan.TotalMinutes):N2} msg/min.");
                    Logging.DebugMessage($"{errorPercent:P} errors");

                    FileSizeTypes totalQ;
                    var rtSize = DateTimeHelper.GetSizeIn(Interlocked.Read(ref receivedTotal), out totalQ);
                    FileSizeTypes lastHourQ;
                    var timeSinceReset = DateTime.UtcNow - resetTime.AddHours(-1);
                    var lhSize = DateTimeHelper.GetSizeIn(Interlocked.Read(ref receivedLastHour), out lastHourQ);
                    Logging.DebugMessage($"{lhSize:N1}{lastHourQ} last {timeSinceReset.TotalMinutes:N0} minute{(timeSinceReset.TotalMinutes < 2 ? "" : "s")} . {rtSize:N1}{totalQ.ToString()} in total");
                    Logging.DebugMessage($"Uptime {(DateTime.UtcNow - collectionSince):g}");

                }
                if (!HomeController.sources.ContainsKey($"{project}.{environment}"))
                {
                    if (!HomeController.sources.ContainsKey($"{project}.{environment}"))
                        HomeController.itemd.Add($"{project}.{environment}");
                    if (!HomeController.sources.ContainsKey($"{project}.{environment}"))
                        HomeController.itemc.Add($"{project}.{environment}");
                    HomeController.sources.TryAdd($"{project}.{environment}", $"{project}.{environment}");
                }
            }
            catch 
            {
                
            }
        }

        public Task AddStreamBatch(string project, string environment, StreamItem[] items)
        {
            LogBytesReceived();
            var key = $"{project}.{environment}";
            Interlocked.Add(ref totalCnt,items.Length);// totalCnt += items.Length;
            Interlocked.Increment(ref cnt);
            try
            {
                AddSource(project, environment);
                foreach (var streamItem in items)
                {

                    AddCommonProperties(streamItem);
                    if (streamItem.LogLevel != LogLevels.Error)
                        Interlocked.Increment(ref dbg);
                    else
                        Interlocked.Increment(ref error);
                    streamItem.Environment = environment;
                    if (streamItem.Timestamp == null) streamItem.Timestamp = DateTime.UtcNow;
                    LogUsage(streamItem, key);
                }
                Startup.hub.Clients.Groups(new List<string> { "All", key }).cmessages(items);
                return Task.FromResult("");
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }

        public Task Options(string project, string environment)
        {
            return Task.FromResult(0);
        }

    }

    internal class UsageItem
    {
        private static Timer timer;
        private long _itemsReceived;

        static UsageItem()
        {
            timer = new Timer(1000) { AutoReset = true };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public UsageItem()
        {

        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                List<UsageItem> messages = new List<UsageItem> { StreamServiceImp.Total.MakeUpdateMessage(), StreamServiceImp.Errors.MakeUpdateMessage() };
                foreach (var service in StreamServiceImp.ActivityPrLocation)
                {
                    foreach (var item in service.Value)
                    {
                        messages.Add(item.Value.MakeUpdateMessage());
                    }
                }
                Startup.hub.Clients.All.usageUpdate(messages);
            }
            catch (Exception ex)
            {
            }
        }

        private UsageItem MakeUpdateMessage()
        {
            var message = new UsageItem { ItemsReceived = Interlocked.Read(ref _itemsReceived), Name = Name, Location = Location, TimeStamp = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1)) };
            Interlocked.Exchange(ref _itemsReceived, 0);
            return message;
        }

        public string Name { get; set; }

        public long ItemsReceived
        {
            get { return _itemsReceived; }
            set { _itemsReceived = value; }
        }

        public string Location { get; set; }

        public DateTime TimeStamp { get; set; }

        public void Increment()
        {
            Interlocked.Increment(ref _itemsReceived);
        }
    }
    public static class DateTimeHelper
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime;
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
        public static double GetSizeIn(long lengthInByte, out FileSizeTypes sizeQualifier)
        {
            try
            {
                List<FileSizeTypes> sizes = EnumHelper.EnumToList<FileSizeTypes>();
                double len = lengthInByte;
                var order = 0;
                while (len >= 1024 && ++order < 5)
                {
                    len = len / 1024;
                }

                sizeQualifier = (FileSizeTypes)order;
                return len;
            }
            catch (Exception ex)
            {
                ex.Log();
                sizeQualifier = FileSizeTypes.MB;
                double len = lengthInByte;
                return len / 1024 / 1024;
            }
        }
    }
    public enum FileSizeTypes
    {
        B = 0,
        kB = 1,
        MB = 2,
        GB = 3,
        TB = 4
    }
}
