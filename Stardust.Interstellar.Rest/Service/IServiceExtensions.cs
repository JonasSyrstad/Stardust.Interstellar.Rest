using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

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
}
