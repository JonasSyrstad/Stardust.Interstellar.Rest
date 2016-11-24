using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Annotations.UserAgent;

namespace Stardust.Interstellar.Config
{
    [StardustConfigAuthentication]
    [IRoutePrefix("api")]
    [Retry(5,500,true)]
    [PerformanceHeaders]
    [FixedClientUserAgent("stardust/1.1 (config client)")]
    public interface IConfigReader
    {
        [HttpGet]
        [Route("ConfigReader/{id}")]
        
        ConfigurationSet Get([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string env = null, [In(InclutionTypes.Path)]string updKey = null);
    }
}
