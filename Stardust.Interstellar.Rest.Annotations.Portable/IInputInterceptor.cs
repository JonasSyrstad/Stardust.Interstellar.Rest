using System.Net;

namespace Stardust.Interstellar.Rest.Extensions
{
    public interface IInputInterceptor
    {

        object Intercept(object result, StateDictionary getState);
        bool Intercept(object[] values, StateDictionary stateDictionary, out string cancellationMessage, out HttpStatusCode statusCode);
    }

    public abstract class InputInterceptorBase : IInputInterceptor
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        protected InputInterceptorBase()
        {
            StatusCode=HttpStatusCode.OK;
            CancelMessage = "";
        }

        public abstract object Intercept(object result, StateDictionary getState);

        public virtual bool Intercept(object[] values, StateDictionary stateDictionary, out string cancellationMessage, out HttpStatusCode statusCode)
        {
            var result= Intercept(values);
            cancellationMessage = CancelMessage;
            statusCode = StatusCode;
            return result;
        }

        protected HttpStatusCode StatusCode { get; set; }

        protected string CancelMessage { get; set; }

        protected abstract bool Intercept(object[] values);
    }
}