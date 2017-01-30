using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Owin;
using Stardust.Continuum.Client;
using Stardust.Continuum.Controllers;
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
                    Clients.Caller.cmessage(new StreamItem
                    {
                        Timestamp = DateTime.UtcNow,
                        Message = $"Disconnected from stream: {@group}",
                        LogLevel = LogLevels.Information,
                        CorrelationToken = "startup",
                        UserName = "streamClient",
                        ServiceName = "LogStream",
                    });
                }
                catch (Exception)
                {

                }
            }
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
        private static int cnt = 0;
        private static long totalCnt = 0;
        private static long dbg = 0;
        private static long error = 0;
        private static DateTime collectionSince=DateTime.UtcNow;

        public Task AddStream(string project, string environment, StreamItem item)
        {
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
                Startup.hub.Clients.Groups(new List<string> { "All", $"{project}.{environment}" }).cmessage(item);
                return Task.FromResult("");
            }
            catch (Exception exception)
            {
                exception.Log();
                throw;
            }
        }

        private static void AddSource(string project, string environment)
        {
            if (cnt > 200)
            {
                cnt = 0;
                var timeSpan = (DateTime.UtcNow - collectionSince);
                Logging.DebugMessage($"{dbg} debug, {error} error messages received. Giving a total of {totalCnt} messages added since {collectionSince}. Average rate {totalCnt/timeSpan.TotalSeconds}m/sec, {totalCnt / timeSpan.TotalMinutes}m/min");

            }
            if (!HomeController.sources.ContainsKey($"{project}.{environment}"))
            {
                HomeController.sources.TryAdd($"{project}.{environment}", $"{project}.{environment}");
                HomeController.itemd.Add($"{project}.{environment}");
            }
        }

        public Task AddStreamBatch(string project, string environment, StreamItem[] items)
        {
            totalCnt += items.Length;
            cnt++;
            try
            {
                AddSource(project, environment);
                foreach (var streamItem in items)
                {
                    if (streamItem.LogLevel != LogLevels.Error)
                        dbg++;
                    else
                        error++;
                    streamItem.Environment = environment;
                    if (streamItem.Timestamp == null) streamItem.Timestamp = DateTime.UtcNow;
                }
                Startup.hub.Clients.All.cmessages(items);
                return Task.FromResult("");
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }
    }
}
