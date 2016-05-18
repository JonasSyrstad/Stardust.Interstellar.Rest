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
    [AttributeUsage(AttributeTargets.Interface|AttributeTargets.Method)]
    class PerformanceHeadersAttribute:HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] {new PerformanceHeadersHandler()};
        }
    }

    

    internal class PerformanceHeadersHandler : StatefullHeaderHandlerBase
    {
        protected override void DoSetHeader(Dictionary<string, object> state, HttpWebRequest req)
        {
            state.Add("sd-timer",Stopwatch.StartNew());
        }

        protected override void DoGetHeader(Dictionary<string, object> state, HttpWebResponse response)
        {
            var sw = ((Stopwatch)state["sd-timer"]);
            sw.Stop();
            
        }
    }
}
