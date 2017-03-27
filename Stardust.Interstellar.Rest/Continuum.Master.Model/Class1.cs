using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;

namespace Continuum.Master.Model
{
    [IRoutePrefix("api/statistics")]
    [ContinuumMasterApiKey]
    public interface INodeStatistics
    {
        [PerformanceHeaders]
        [Route("{node}/{target}/")]
        [HttpPatch]
        Task AddStatistics([In(InclutionTypes.Path) ]string node, [In(InclutionTypes.Path)]string target, [In(InclutionTypes.Body)]StatisticsItem statistics);
    }

    [IRoutePrefix("api/key")]
    [ContinuumMasterApiKey]
    public interface IApiKeyValidator
    {
        [PerformanceHeaders]
        [Route("{node}/{target}/validate")]
        [HttpPost]
        Task<bool> IsThisAValidApiKey([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]string apiKey);

        //[PerformanceHeaders]
        //[Route("")]

    }

    [IRoutePrefix("api/users")]
    [ContinuumMasterApiKey]
    public interface IUserService
    {
        [HttpGet]
        [Route("validate/{username}/{target}")]
        Task<bool> HasAccessTo(string username, string target);



    }

    public class ContinuumMasterApiKeyAttribute:AuthenticationInspectorAttributeBase, IAuthenticationHandler
    {
        private string _apiKey;

        public override IAuthenticationHandler GetHandler()
        {
            return this;
        }

        public void Apply(HttpWebRequest req)
        {
            if (_apiKey == null)
            {
                _apiKey =Convert.ToBase64String(Encoding.UTF8.GetBytes( ApiKey()));

            }
        }

        private static string ApiKey()
        {
            return $"{ConfigurationManager.AppSettings["continuum:master:key"]}/{ConfigurationManager.AppSettings["continuum:master:url"]}/{ConfigurationManager.AppSettings["authority"]}";
        }
    }

    public class StatisticsItem
    {
    }
}
