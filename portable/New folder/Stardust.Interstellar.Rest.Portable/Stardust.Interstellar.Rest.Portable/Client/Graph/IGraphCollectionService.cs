using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.Annotations;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    

    [IRoutePrefix("graph", true)]
    public interface IGraphCollectionService<T>
    {
        [HttpGet]
        [Route("{id}")]
        Task<T> GetAsync([In(InclutionTypes.Path)] string id);

        [HttpPost]
        [Route]
        Task<IEnumerable<T>> QueryAsync([In(InclutionTypes.Body)] GraphQuery queryExpression);

        [HttpGet]
        [Route]
        Task<IEnumerable<T>> GetAllAsync();

        [HttpPut]
        [Route("")]
        Task AddAsync(T item);

        [HttpDelete]
        [Route("{id}")]
        Task RemoveAsync([In(InclutionTypes.Path)]string id);

        [HttpPost]
        [Route("{id}")]
        Task UpdateAsync([In(InclutionTypes.Path)]string id, [In(InclutionTypes.Body)]T item);

        [HttpGet]
        [Route("{id}/{graphNodes}")]
        Task<IEnumerable<T>> GetGraphNodesAsync([In(InclutionTypes.Path)]string id, [In(InclutionTypes.Path)]string graphNodes);

    }
}