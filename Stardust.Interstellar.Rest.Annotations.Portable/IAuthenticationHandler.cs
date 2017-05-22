using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Stardust.Interstellar.Rest.Extensions
{
    public interface IAuthenticationHandler
    {
        void Apply(HttpWebRequest req);
    }
}