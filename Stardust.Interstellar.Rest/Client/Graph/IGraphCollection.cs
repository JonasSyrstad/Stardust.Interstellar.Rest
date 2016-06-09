using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public interface IPagedGraphCollection<T> : IGraphCollection<IGraphItem<T>>
    {
        int PageSize { get; set; }

        IGraphCollection<IGraphItem<T>> CurrentPage { get; }

        bool Next();

        Task<bool> NextAsync();

        void Reset();
    }

    class PagedGraphCollection<T> : GraphCollection<IGraphItem<T>>,IPagedGraphCollection<T>
    {
        public int PageSize { get; set; }

        public int CurrentPageNo { get; private set; }

        public IGraphCollection<IGraphItem<T>> CurrentPage { get; }

        public bool Next()
        {
            CurrentPageNo++;
            return true;
        }

        public async Task<bool> NextAsync()
        {
            CurrentPageNo++;
            return true;
        }

        public void Reset()
        {
            CurrentPageNo = 0;
        }
    }

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