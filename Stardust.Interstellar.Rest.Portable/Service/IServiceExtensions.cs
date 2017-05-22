using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Linq;
using Stardust.Interstellar.Rest.Annotations.Messaging;
using Stardust.Interstellar.Rest.Common;

namespace Stardust.Interstellar.Rest.Service
{
    /// <summary>
    /// Append this interface to the service implementation, to gain access to the controller context.
    /// </summary>
    public interface IServiceExtensions
    {
        void SetControllerContext(HttpControllerContext currentContext);

        void SetResponseHeaderCollection(Dictionary<string, string> headers);

        Dictionary<string, string> GetHeaders();
    }

    public static class MessageExtensions
    {
        public static JObject GetExtendedMessage(this IServiceWithGlobalParameters service)
        {
            var state = GetCache().GetState();
            return state;
        }

        public static IStateCache GetCache()
        {
            var cache = ExtensionsFactory.GetService<IStateCache>();
            if (cache == null) return new HttpContextMessageContainer();
            return cache;
        }
    }

    public interface IStateCache
    {
        JObject GetState();

        void SetState(JObject extendedMessage);
    }
}
