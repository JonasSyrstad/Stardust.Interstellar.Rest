using System;
using System.Collections.Generic;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public interface IInternalGraphHelper
    {
        string BaseUrl { get; set; }

        IGraphItem Parent { get; set; }


        void SetExtrasHandler(Action<Dictionary<string, object>> handler);

    }
}