using System.Net;

namespace Stardust.Interstellar.Rest
{
    public class NullAuthHandler : IAuthenticationHandler
    {
        public void Apply(HttpWebRequest req)
        {

        }
    }
}