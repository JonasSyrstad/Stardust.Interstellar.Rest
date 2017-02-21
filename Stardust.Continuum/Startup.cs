using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Http.Controllers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Owin;
using Stardust.Continuum.Client;
using Stardust.Continuum.Controllers;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;

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
                EnableJavaScriptProxies = true
            });
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
        private static int cnt = 0;
        private static long totalCnt = 0;
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
            AddCommonProperties(item);
            totalCnt++;
            cnt++;
            if (item.LogLevel != LogLevels.Error)
                dbg++;
            else
                error++;
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
            var size = HttpContext.Current.Request.ContentLength;
            receivedTotal += size;
            if (resetTime < DateTime.UtcNow)
            {
                resetTime = DateTime.UtcNow.AddHours(1);
                receivedLastHour = 0;
            }
            receivedLastHour += size;
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
                locationCounter.ItemsReceived++;
                Total.ItemsReceived++;
                if (item.LogLevel == LogLevels.Error)
                    Errors.ItemsReceived++;
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
            if (cnt > 200)
            {

                var errorPercent = totalCnt > 0 ? (error / totalCnt) * 100 : 0;
                cnt = 0;
                var timeSpan = (DateTime.UtcNow - collectionSince);
                Logging.DebugMessage($"{DateTime.UtcNow}: {dbg:N0} debug, {error:N0} error and total {totalCnt:N0}. {(totalCnt / timeSpan.TotalSeconds):N2} msg/sec, {(totalCnt / timeSpan.TotalMinutes):N2} msg/min.");
                Logging.DebugMessage($"{errorPercent:P} errors");

                FileSizeTypes totalQ;
                var rtSize = DateTimeHelper.GetSizeIn(receivedTotal, out totalQ);
                FileSizeTypes lastHourQ;
                var timeSinceReset = DateTime.UtcNow - resetTime.AddHours(-1);
                var lhSize = DateTimeHelper.GetSizeIn(receivedLastHour, out lastHourQ);
                Logging.DebugMessage($"{lhSize:N1}{lastHourQ.ToString()} last {timeSinceReset.TotalMinutes:N0} minute{(timeSinceReset.TotalMinutes<2?"":"s")} . {rtSize:N1}{totalQ.ToString()} in total");
                Logging.DebugMessage($"Uptime {(DateTime.UtcNow - collectionSince):g}");

            }
            if (!HomeController.sources.ContainsKey($"{project}.{environment}"))
            {
                HomeController.sources.TryAdd($"{project}.{environment}", $"{project}.{environment}");
                HomeController.itemd.Add($"{project}.{environment}");
                HomeController.itemc.Add($"{project}.{environment}");
            }
        }

        public Task AddStreamBatch(string project, string environment, StreamItem[] items)
        {
            LogBytesReceived();
            var key = $"{project}.{environment}";
            totalCnt += items.Length;
            cnt++;
            try
            {
                AddSource(project, environment);
                foreach (var streamItem in items)
                {
                    
                    AddCommonProperties(streamItem);
                    if (streamItem.LogLevel != LogLevels.Error)
                        dbg++;
                    else
                        error++;
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
            var message = new UsageItem { ItemsReceived = ItemsReceived, Name = Name, Location = Location, TimeStamp = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1)) };
            ItemsReceived = 0;
            return message;
        }

        public string Name { get; set; }

        public long ItemsReceived { get; set; }
        public string Location { get; set; }

        public DateTime TimeStamp { get; set; }
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
