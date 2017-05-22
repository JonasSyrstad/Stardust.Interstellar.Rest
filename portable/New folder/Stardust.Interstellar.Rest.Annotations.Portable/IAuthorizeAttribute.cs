using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class IAuthorizeAttribute : Attribute
    {

        //
        // Summary:
        //     Gets or sets the authorized roles.
        //
        // Returns:
        //     The roles string.
        public string Roles { get; set; }
        
        //
        // Summary:
        //     Gets or sets the authorized users.
        //
        // Returns:
        //     The users string.
        public string Users { get; set; }

    }
}