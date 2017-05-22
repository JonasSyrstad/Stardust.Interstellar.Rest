using System;

namespace Stardust.Interstellar.Rest.Annotations
{
    public abstract class VerbAttribute : Attribute
    {
        protected VerbAttribute()
        {
            
        }

        protected VerbAttribute(string verb)
        {
            Verb = verb;
        }
        public string Verb { get; private set; }
    }
}