using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.Annotations;

namespace Stardust.Continuum.Client
{
    [IRoutePrefix("api/v1")]
    [ApiKey]
    [CircuitBreaker(10, 3, 10)]
    [PerformanceHeaders]
    [CallingMachineName]
    [IAuthorize]
    public interface ILogStream
    {
        [HttpPut]
        [Route("single/{project}/{environment}")]
        Task AddStream([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]StreamItem item);

        [Route("batch/{project}/{environment}")]
        [HttpPut]
        Task AddStreamBatch([In(InclutionTypes.Path)]string project, [In(InclutionTypes.Path)]string environment, [In(InclutionTypes.Body)]StreamItem[] items); 
    }
}