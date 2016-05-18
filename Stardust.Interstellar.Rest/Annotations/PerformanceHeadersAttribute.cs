using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method|AttributeTargets.Assembly)]
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

        protected override void DoSetHeader(StateDictionary state, HttpWebRequest req)
        {
            if(state.ContainsKey(StardustTimerKey)) return;
            state.SetState(StardustTimerKey, Stopwatch.StartNew());
        }

        protected override void DoGetHeader(StateDictionary state, HttpWebResponse response)
        {
            var sw = state.GetState<Stopwatch>(StardustTimerKey);
            sw.Stop();
            var server = response.Headers[StardustTimerKey];
            if (!string.IsNullOrWhiteSpace(server))
            {
                var serverTime = long.Parse(server);
                var latency = sw.ElapsedMilliseconds - serverTime;
                state.Extras.Add("latency",latency);
                state.Extras.Add("serverTime",serverTime);
                state.Extras.Add("totalTime",sw.ElapsedMilliseconds);
            }

        }

        protected override void DoSetServiceHeaders(StateDictionary state, HttpResponseHeaders headers)
        {
            var sw =  state.GetState<Stopwatch>(StardustTimerKey);
            sw.Stop();
            headers.Add(StardustTimerKey, sw.ElapsedMilliseconds.ToString());
        }

        protected override void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers)
        {
            state.Add(StardustTimerKey, Stopwatch.StartNew());
        }
    }
}
