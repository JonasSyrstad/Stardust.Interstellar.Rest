using System.Net;
using Stardust.Interstellar.Rest.Annotations;

namespace Stardust.Interstellar.Rest.Extensions
{
    public class NullAuthHandler : IAuthenticationHandler
    {
        public void Apply(IRequestWrapper req)
        {

        }
    }
}