using System;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method|AttributeTargets.Assembly)]
    public class PerformanceHeadersAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new IHeaderHandler[] { new PerformanceHeadersHandler() };
        }
    }
}
