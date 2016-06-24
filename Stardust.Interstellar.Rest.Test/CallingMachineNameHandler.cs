using System;
using System.Net;
using System.Net.Http.Headers;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Test
{
    public class CallingMachineNameHandler : IHeaderHandler
    {
        /// <summary>
        /// The order of execution. Lower numbers will be processed first
        /// </summary>
        public int ProcessingOrder => -1;

        public void SetHeader(HttpWebRequest req)
        {
            req.Headers.Add("x-callingMachine", Environment.MachineName);
        }

        public void GetHeader(HttpWebResponse response)
        {

        }

        public void GetServiceHeader(HttpRequestHeaders headers)
        {

        }

        public void SetServiceHeaders(HttpResponseHeaders headers)
        {
            
        }
    }
}