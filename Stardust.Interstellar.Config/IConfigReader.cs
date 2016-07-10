using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest;
using Stardust.Interstellar.Rest.Annotations;

namespace Stardust.Interstellar.Config
{
    [StardustConfigAuthentication]
    [IRoutePrefix("api")]
    public interface IConfigReader
    {
        [HttpGet]
        [Route("ConfigReader/{id}")]
        
        ConfigurationSet Get([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string env = null, [In(InclutionTypes.Path)]string updKey = null);
    }
}
