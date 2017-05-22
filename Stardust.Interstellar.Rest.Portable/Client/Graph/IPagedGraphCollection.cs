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
}