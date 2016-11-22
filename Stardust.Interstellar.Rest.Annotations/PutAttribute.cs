using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Method )]
    public class PutAttribute : VerbAttribute
    {
        public PutAttribute() : base("PUT")
        {
        }
    }
}