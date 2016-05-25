using System.Net;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public class AuthHandler : IAuthenticationHandler
    {
        private NetworkCredential credentials;

        private string bearerToken;

        public void Apply(HttpWebRequest req)
        {
            if (bearerToken.ContainsCharacters())
                req.Headers.Add("Authorization", "Bearer " + bearerToken);
            else if (credentials != null) req.Credentials = credentials;
            else
            {
                var tokenManager = Resolver.Activate<IOAuthTokenProvider>();
                bearerToken = tokenManager?.GetBearerToken();
            }
        }

        internal void SetNetworkCredentials(NetworkCredential credential)
        {
            credentials = credential;
        }

        internal void SetBearerToken(string token)
        {
            bearerToken = token;
        }
    }
}