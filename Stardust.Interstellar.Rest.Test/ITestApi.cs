using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;

namespace Stardust.Interstellar.Rest.Test
{
    [IRoutePrefix("api")]
    [CallingMachineName]
    [PerformanceHeaders]
    [ErrorHandler(typeof(TestHandler))]
    public interface ITestApi
    {
        [Route("test/{id}")]
        [HttpGet]
        string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);

        [Route("test2/{id}")]
        [HttpGet]
        string Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

        [Route("test3/{id}")]
        [HttpGet]
        string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

        [Route("put1/{id}")]
        [HttpPut]
        void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("test5/{id}")]
        [HttpGet]
        Task<StringWrapper> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

        [Route("put2/{id}")]
        [HttpPut]
        Task PutAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("failure/{id}")]
        [HttpPut]
        Task FailingAction([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] string timestamp);
    }

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
            if (!(exception is AggregateException)) return null;
            {
                if (!((exception as AggregateException).InnerExceptions is NotImplementedException))
                {
                    var resp = request.CreateResponse(HttpStatusCode.BadGateway, DateTime.Now);
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