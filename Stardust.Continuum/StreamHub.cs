using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Stardust.Continuum.Client;
using Stardust.Continuum.Controllers;

namespace Stardust.Continuum
{
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
}