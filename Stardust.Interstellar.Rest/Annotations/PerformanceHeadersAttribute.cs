using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class PerformanceHeadersAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] { new PerformanceHeadersHandler() };
        }
    }



    internal class PerformanceHeadersHandler : StatefullHeaderHandlerBase
    {
        private const string StardustTimerKey = "x-stardusttimer";

        protected override void DoSetHeader(Dictionary<string, object> state, HttpWebRequest req)
        {
            if(state.ContainsKey(StardustTimerKey)) return;
            state.Add(StardustTimerKey, Stopwatch.StartNew());
        }

        protected override void DoGetHeader(Dictionary<string, object> state, HttpWebResponse response)
        {
            var sw = ((Stopwatch)state[StardustTimerKey]);
            sw.Stop();
            var server = response.Headers[StardustTimerKey];
            if (!string.IsNullOrWhiteSpace(server))
            {
                var serverTime = long.Parse(server);
                var latency = sw.ElapsedMilliseconds - serverTime;
                state.Extras().Add("latency",latency);
                state.Extras().Add("serverTime",serverTime);
                state.Extras().Add("totalTime",sw.ElapsedMilliseconds);
            }

        }

        protected override void DoSetServiceHeaders(Dictionary<string, object> state, HttpResponseHeaders headers)
        {
            var sw = ((Stopwatch)state[StardustTimerKey]);
            sw.Stop();
            headers.Add(StardustTimerKey, sw.ElapsedMilliseconds.ToString());
        }

        protected override void DoGetServiceHeader(Dictionary<string, object> state, HttpRequestHeaders headers)
        {
            state.Add(StardustTimerKey, Stopwatch.StartNew());
        }
    }
}
