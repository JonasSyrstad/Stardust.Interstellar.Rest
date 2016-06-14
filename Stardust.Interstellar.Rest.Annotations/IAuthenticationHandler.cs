using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Stardust.Interstellar.Rest.Extensions
{
    public interface IAuthenticationHandler
    {
        void Apply(HttpWebRequest req);
    }

    public interface IInputInterceptor
    {
        void Intercept(object[] inputs, StateDictionary getState, out bool cancel, out string cancellationMessage);

        object Intercept(object result, StateDictionary getState);
    }

    public abstract class InputInterceptorAttribute : Attribute
    {
        public abstract IInputInterceptor GetInterceptor();
    }
}