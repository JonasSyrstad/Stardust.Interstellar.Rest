using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stardust.Continuum.Client;
using Xunit;

namespace Continuum.Test
{
    public class StreamClientTests
    {
        static StreamClientTests()
        {
            ContinuumClient.LimitMessageSize = 500;
            ContinuumClient.BaseUrl = "http://continuumdemo.azurewebsites.net";
            ContinuumClient.Project = "test";
            ContinuumClient.Environment = "test";
            ContinuumClient.SetApiKey("test123");
        }

        [Fact]
        public async Task AddBufferedEvents()
        {
            for (int y = 0; y < 20; y++)
            {
                for (int i = 0; i < 1000; i++)
                {
                    ContinuumClient.AddStream(new StreamItem
                    {
                        Message = "test " + i,
                        ServiceName = "unittest",
                        CorrelationToken = "test",
                        Properties = new Dictionary<string, object>(),
                        UserName = "xunit",
                        LogLevel = LogLevels.Debug,
                        Timestamp = DateTime.UtcNow

                    });
                }
                await Task.Delay(100);

            }

            await Task.Delay(1000);

        }
    }
}
