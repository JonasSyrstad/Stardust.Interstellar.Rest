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
                bearerToken = tokenManager?.GetBearerToken(req.RequestUri.ToString());
                if(bearerToken.IsNullOrWhiteSpace()) return;
                bearerToken = tokenManager?.GetBearerToken(req.GetState().Extras.GetState<string>("serviceRoot")??req.RequestUri.ToString());
                req.Headers.Add("Authorization", "Bearer " + bearerToken);
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