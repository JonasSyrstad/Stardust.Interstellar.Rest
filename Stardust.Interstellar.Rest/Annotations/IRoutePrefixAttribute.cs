using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class IRoutePrefixAttribute : Attribute
    {
        private readonly string prefix;

        public IRoutePrefixAttribute(string prefix)
        {
            this.prefix = prefix;
        }

        public string Prefix
        {
            get
            {
                return prefix;
            }
        }
    }
}