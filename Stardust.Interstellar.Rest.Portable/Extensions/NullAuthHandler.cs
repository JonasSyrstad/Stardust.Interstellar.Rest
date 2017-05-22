using System.Net;

namespace Stardust.Interstellar.Rest.Extensions
{
    public class NullAuthHandler : IAuthenticationHandler
    {
        public void Apply(HttpWebRequest req)
        {

        }
    }
}