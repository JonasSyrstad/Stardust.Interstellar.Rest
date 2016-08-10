using System.Linq;
using System.Text;
using Microsoft.JScript;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public abstract class GraphContext<T> :GraphItem<T>,IGraphContext
    {
        protected GraphContext(string baseUrl)
        {
            InternalBaseUrl = baseUrl;
        }

        protected GraphContext() { }

        public void Initialize(string baseUrl)
        {
            InternalBaseUrl = baseUrl;
        }
    }

    public interface IGraphContext
    {
        void Initialize(string baseUrl);
    }
}
