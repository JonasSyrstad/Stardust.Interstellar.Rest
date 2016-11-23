using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;

namespace Stardust.Interstellar.Rest.Test
{
    public class TestHandler : IErrorHandler
    {
        /// <summary>
        /// Provide additional excption type to http status code mappings mappings 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResponseMessage ConvertToErrorResponse(Exception exception, HttpRequestMessage request)
        {
            Logging.DebugMessage(exception.GetType().FullName);
            if (exception is NotImplementedException)
            {
                var resp= request.CreateResponse(HttpStatusCode.InternalServerError,DateTime.Now);
                return resp;
            }
            if (exception is StatusException)
            {
                var resp = request.CreateResponse(HttpStatusCode.RequestTimeout, DateTime.Now);
                return resp;
            }
            if (!(exception is AggregateException)) return null;
            {
                if (!((exception as AggregateException).InnerExceptions is NotImplementedException))
                {
                    var resp = request.CreateResponse(HttpStatusCode.BadGateway, DateTime.Now);
                    return resp;
                }
                if (!((exception as AggregateException).InnerExceptions is StatusException))
                {
                    var resp = request.CreateResponse(HttpStatusCode.RequestTimeout, DateTime.Now);
                    return resp;
                }
            }

            return null;
        }

        /// <summary>
        /// Set to true if this handles all exception types 
        /// </summary>
        public bool OverrideDefaults
        {
            get
            {
                return true;
            }
        }

        public Exception ProduceClientException(string statusMessage, HttpStatusCode status, Exception error, string value)
        {
            DateTime? msg=null;
            try
            {

                if (string.IsNullOrWhiteSpace(value)) msg = null;
                else
                    msg = JsonConvert.DeserializeObject<DateTime>(value);
            }
            catch (Exception)
            {

            }
            return new RestWrapperException(statusMessage,status,msg,error);
        }
    }
}