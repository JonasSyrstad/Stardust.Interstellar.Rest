using System;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class StardustConfigAuthenticationAttribute: AuthenticationInspectorAttributeBase
    {
        public override IAuthenticationHandler GetHandler()
        {
            return new StardustConfigAuthentication();
        }
    }
}