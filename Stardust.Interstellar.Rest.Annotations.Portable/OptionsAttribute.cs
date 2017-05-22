using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsAttribute : VerbAttribute
    {
        public OptionsAttribute() : base("OPTIONS")
        {
        }
    }
}