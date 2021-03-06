using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Annotations
{
    internal class PerformanceHeadersHandler : StatefullHeaderHandlerBase
    {
        private const string StardustTimerKey = "x-stardusttimer";

        /// <summary>
        /// The order of execution. Lower numbers will be processed first
        /// </summary>
        public override int ProcessingOrder => -2;

        protected override void DoSetHeader(StateDictionary state, IRequestWrapper req)
        {
            if(state.ContainsKey(StardustTimerKey)) return;
            state.SetState(StardustTimerKey, Stopwatch.StartNew());
        }

        protected override void DoGetHeader(StateDictionary state, IResponseWrapper response)
        {
            var sw = state.GetState<Stopwatch>(StardustTimerKey);
            if(sw==null) return;
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
            if(sw==null) return;
            sw.Stop();
            headers.Add(StardustTimerKey, sw.ElapsedMilliseconds.ToString());
        }

        protected override void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers)
        {
            state.Add(StardustTimerKey, Stopwatch.StartNew());
        }
    }
}