using System;
using System.Web.Http;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class IAuthorizeAttribute : AuthorizeAttribute
    {

    }
}