using System.Net;


namespace Stardust.Interstellar.Rest.Extensions
{
    public interface IAuthenticationHandler
    {
        void Apply(HttpWebRequest req);
    }
}