using System.Security.Principal;

namespace Stardust.Interstellar
{
    public interface IOAuthBearerTokenValidator
    {
        IPrincipal Validate(string token);
    }
}