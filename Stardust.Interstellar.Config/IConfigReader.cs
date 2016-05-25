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
    public interface IConfigReader
    {
        [HttpGet]
        [Route("api/ConfigReader/{id}")]
        
        ConfigurationSet Get([FromUri] string id, [In(InclutionTypes.Path)]string env = null, [In(InclutionTypes.Path)]string updKey = null);
    }
}
