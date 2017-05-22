using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class PostAttribute : VerbAttribute
    {
        public PostAttribute() : base("POST")
        {
        }
    }
}