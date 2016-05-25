using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public interface IGraphItem<T> : IGraphItem
    {
        Task<T> GetAsync();

        Task DeleteAsync();

        Task SaveAsync();

        T Value { get; }
    }
    public interface IGraphItem
    {
        IGraphItem Initialize(IGraphItem parent);


    }
}