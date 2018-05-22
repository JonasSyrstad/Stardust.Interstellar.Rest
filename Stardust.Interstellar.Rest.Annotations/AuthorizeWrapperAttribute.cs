using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Interstellar.Rest.Annotations
{
    /// <summary>
    /// Authorize attribute wrapper. Allows the framework to inject the correct authorice attribute on the service implementation.
    /// Override this and return the webapi authorize attribute (or your custom auth code)
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeWrapperAttribute : Attribute
    {
        private object[] ctorParams;

        public virtual AuthorizeAttribute GetAutorizationAttribute()
        {
            return new AuthorizeAttribute { Roles = Roles, Users = Users };
        }


        public object[] GetConstructorParameters()
        {
            return ctorParams;
        }

        public AuthorizeWrapperAttribute(params object[] constructorParams)
        {
            ctorParams = constructorParams ?? new object[0];
        }

        public string Users { get; set; }

        public string Roles { get; set; }
    }


}
