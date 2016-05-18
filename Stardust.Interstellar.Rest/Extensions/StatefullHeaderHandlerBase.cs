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
        /// Set custom header values on sending request to a service
        /// </summary>
        /// <param name="req"></param>
        public void SetHeader(HttpWebRequest req)
        {
            var state = req.GetState();
            DoSetHeader(state, req);
        }


        protected abstract void DoSetHeader(Dictionary<string, object> state, HttpWebRequest req);

        /// <summary>
        /// Get header values form a service response
        /// </summary>
        /// <param name="response"></param>
        public void GetHeader(HttpWebResponse response)
        {
            var state = response.GetState();
            DoGetHeader(state, response);
        }

       

        protected abstract void DoGetHeader(Dictionary<string, object> state, HttpWebResponse response);
        

        protected abstract void DoSetServiceHeaders(Dictionary<string, object> state, HttpResponseHeaders headers);

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

        protected abstract void DoGetServiceHeader(Dictionary<string, object> state, HttpRequestHeaders headers);
    }
}
