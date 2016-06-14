using System;
using System.Collections.Generic;
using System.Net.Http;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Common
{
    internal class ActionWrapper
    {
        public string Name { get; set; }

        public Type ReturnType { get; set; }

        public List<ParameterWrapper> Parameters { get; set; }

        public string RouteTemplate { get; set; }

        public List<HttpMethod> Actions { get; set; }

        public List<IHeaderHandler> CustomHandlers { get; set; }

        public InputInterceptorAttribute[] Interceptor { get; set; }
    }
}