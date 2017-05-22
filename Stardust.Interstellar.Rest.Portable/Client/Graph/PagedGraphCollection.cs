using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
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
}