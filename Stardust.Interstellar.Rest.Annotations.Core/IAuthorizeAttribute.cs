using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class IAuthorizeAttribute : Attribute
    {
        public string Users { get; set; }

        public string Roles { get; set; }
    }
}