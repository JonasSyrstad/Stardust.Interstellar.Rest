using System.Security.Principal;
using System.Web.Http.Controllers;

namespace Stardust.Interstellar
{
    public interface IRequestAuthenticator
    {
        /// <summary>
        /// Do custom authorization of the inncomming request
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        IPrincipal AuthorizeRequest(HttpActionContext actionContext);
    }
}