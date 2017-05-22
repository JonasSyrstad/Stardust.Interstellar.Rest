using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph.InternalHelpers
{
   public static  class GraphHelpers
    {
       public static void SetExtrasHandlerInternal(this IInternalGraphHelper graph,Action<Dictionary<string, object>> handler)
       {
           graph.SetExtrasHandler(handler);
       }
    }
}
