using System;
using System.Net;
using System.Net.Http.Headers;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Annotations.UserAgent
{
    public class FixedClientUserAgentAttribute : Attribute, IHeaderHandler, IHeaderInspector
    {
        private readonly string userAgentName;

        public FixedClientUserAgentAttribute(string userAgentName)
        {
            this.userAgentName = userAgentName;
        }

        /// <summary>
        /// Set custom header values on sending request to a service
        /// </summary>
        /// <param name="req"></param>
        void IHeaderHandler.SetHeader(HttpWebRequest req)
        {
            req.UserAgent = userAgentName;
        }

        /// <summary>
        /// Get header values form a service response
        /// </summary>
        /// <param name="response"></param>
        void IHeaderHandler.GetHeader(HttpWebResponse response)
        {

        }

        /// <summary>
        /// Get custom header values received from the client 
        /// </summary>
        /// <param name="headers"></param>
        void IHeaderHandler.GetServiceHeader(HttpRequestHeaders headers)
        {

        }

        void IHeaderHandler.SetServiceHeaders(HttpResponseHeaders headers)
        {

        }

        IHeaderHandler[] IHeaderInspector.GetHandlers()
        {
            return new IHeaderHandler[] { new FixedClientUserAgentAttribute(userAgentName) };
        }
    }
}
