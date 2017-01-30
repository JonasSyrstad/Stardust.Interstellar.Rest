using System;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Continuum.Client
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public sealed class CallingMachineNameAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] { new CallingMachineNameHandler() };
        }
    }
}