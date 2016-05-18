using System.Net;
using System.Net.Http.Headers;

namespace Stardust.Interstellar.Rest
{
    public interface IHeaderHandler
    {
        /// <summary>
        /// Set custom header values on sending request to a service
        /// </summary>
        /// <param name="req"></param>
        void SetHeader(HttpWebRequest req);

        /// <summary>
        /// Get header values form a service response
        /// </summary>
        /// <param name="response"></param>
        void GetHeader(HttpWebResponse response);

        /// <summary>
        /// Set custom header values before sending the response from the service
        /// </summary>
        /// <param name="headers"></param>
        void SetServiceHeaders(WebHeaderCollection headers);

        /// <summary>
        /// Get custom header values received from the client 
        /// </summary>
        /// <param name="headers"></param>
        void GetServiceHeader(HttpRequestHeaders headers);
    }
}