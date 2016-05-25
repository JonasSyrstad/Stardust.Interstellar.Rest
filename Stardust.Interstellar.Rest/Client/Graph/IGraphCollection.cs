using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public interface IGraphCollection<T> : IEnumerable<T>
    {
        T this[string id] { get; set; }

        Task AddAsync(T item);

        Task DeleteAsync(string id);

        Task<T> GetAsync(string id);

        Task UpdateAsync(string id, T item);


        Task<IEnumerable<T>> QueryAsync(GraphQuery query);

    }
}