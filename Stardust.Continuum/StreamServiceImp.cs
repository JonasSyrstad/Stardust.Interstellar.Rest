using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Stardust.Continuum.Client;
using Stardust.Continuum.Controllers;
using Stardust.Particles;

namespace Stardust.Continuum
{
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
}