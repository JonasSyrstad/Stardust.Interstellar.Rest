namespace Stardust.Interstellar.Rest.Client.Graph
{
    public interface IInternalGraphHelper
    {
        string BaseUrl { get; set; }

        IGraphItem Parent { get; set; }
    }
}