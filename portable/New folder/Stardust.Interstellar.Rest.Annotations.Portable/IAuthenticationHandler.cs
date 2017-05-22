using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace Stardust.Interstellar.Rest.Extensions
{
    public interface IAuthenticationHandler
    {
        void Apply(IRequestWrapper req);
    }

    public interface IRequestWrapper
    {
        IHeaderWrapper Headers { get; set; }
        string UserAgent { get; set; }
    }

    public interface IResponseWrapper
    {
        IHeaderWrapper Headers { get; set; }
    }

    public interface IHeaderWrapper
    {
        string this[string key] { get; set; }
    }

    public interface IRequestMessageWrapper
    { }

    public interface IResponseMessageWrapper
    { }
}