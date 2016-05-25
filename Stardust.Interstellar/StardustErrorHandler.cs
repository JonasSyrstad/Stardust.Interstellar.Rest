using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using Newtonsoft.Json;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public class StardustErrorHandler : IErrorHandler
    {
        private readonly IRuntime runtime;

        public StardustErrorHandler(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        public HttpResponseMessage ConvertToErrorResponse(Exception exception, HttpRequestMessage request)
        {
            runtime.TearDown(exception);
            var tracer = runtime.GetTracer();
            var msg = new ErrorMessage { Message = exception.Message, FaultLocation = tracer?.GetCallstack()?.ErrorPath, TicketNumber = runtime.InstanceId, Detail = ErrorDetail.GetDetails(exception) };
            return request.CreateResponse(HttpStatusCode.InternalServerError, msg);
        }

        public Exception ProduceClientException(string statusMessage, HttpStatusCode status, Exception error, string value)
        {
            if (value.ContainsCharacters())
            {
                return new FaultException<ErrorMessage>(JsonConvert.DeserializeObject<ErrorMessage>(value), new FaultReason(statusMessage));
            }
            return null;
        }

        public bool OverrideDefaults => true;
    }
}