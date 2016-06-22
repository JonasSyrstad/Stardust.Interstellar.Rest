using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using Newtonsoft.Json;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Allows the throwing party to specify the http status code in the response
    /// </summary>
    public class StatusException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public StatusException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public StatusException(HttpStatusCode statusCode,string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public StatusException(HttpStatusCode statusCode,string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        protected StatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
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
            return request.CreateResponse(ConvertToStatusCode(exception), msg);
        }
        
        private static HttpStatusCode ConvertToStatusCode(Exception exception)
        {
            if(exception is NullReferenceException||exception is IndexOutOfRangeException)
                return HttpStatusCode.NotFound;
            if(exception is UnauthorizedAccessException)
                return HttpStatusCode.Unauthorized;
            if(exception is NotImplementedException) return HttpStatusCode.NotImplemented;
            var statusException = exception as StatusException;
            if (statusException != null) return statusException.StatusCode;
            if(exception is AccessViolationException) return HttpStatusCode.Forbidden;
            if (exception is AggregateException) return ConvertToStatusCode(exception.InnerException);
            if(exception is ArgumentException) return HttpStatusCode.BadRequest;
            return HttpStatusCode.InternalServerError;
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