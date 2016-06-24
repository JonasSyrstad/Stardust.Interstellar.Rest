using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Extensions
{
    public abstract class StatefullHeaderHandlerBase : IHeaderHandler
    {
        /// <summary>
        /// The order of execution. Lower numbers will be processed first
        /// </summary>
        public abstract int ProcessingOrder { get; }

        /// <summary>
        /// Set custom header values on sending request to a service
        /// </summary>
        /// <param name="req"></param>
        public void SetHeader(HttpWebRequest req)
        {
            var state = req.GetState();
            DoSetHeader(state, req);
        }


        protected abstract void DoSetHeader(StateDictionary state, HttpWebRequest req);

        /// <summary>
        /// Get header values form a service response
        /// </summary>
        /// <param name="response"></param>
        public void GetHeader(HttpWebResponse response)
        {
            try
            {
                var state = response.GetState();
                DoGetHeader(state, response);
            }
            catch (Exception)
            {
                // ignored
            }
        }

       

        protected abstract void DoGetHeader(StateDictionary state, HttpWebResponse response);
        

        protected abstract void DoSetServiceHeaders(StateDictionary state, HttpResponseHeaders headers);

        /// <summary>
        /// Get custom header values received from the client 
        /// </summary>
        /// <param name="headers"></param>
        public void GetServiceHeader(HttpRequestHeaders headers)
        {
            var state = headers.GetState();
            DoGetServiceHeader(state, headers);
        }

        public void SetServiceHeaders(HttpResponseHeaders headers)
        {
            var state = headers.GetState();
            DoSetServiceHeaders(state, headers);
        }
        

        protected abstract void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers);
    }
}
