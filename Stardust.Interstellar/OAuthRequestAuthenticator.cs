using System;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Stardust.Interstellar.Rest.Common;

namespace Stardust.Interstellar
{
    public class OAuthRequestAuthenticator:IRequestAuthenticator
    {
        /// <summary>
        /// Do custom authorization of the inncomming request
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public IPrincipal AuthorizeRequest(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            
            if (auth!=null && auth.Scheme.Equals("bearer",StringComparison.InvariantCultureIgnoreCase))
            {
                var token = auth.Parameter;
                return ExtensionsFactory.GetService<IOAuthBearerTokenValidator>().Validate(token);
            }
            return null;
        }
    }
}