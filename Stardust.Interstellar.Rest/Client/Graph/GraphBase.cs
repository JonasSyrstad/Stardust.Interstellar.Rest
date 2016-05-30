namespace Stardust.Interstellar.Rest.Client.Graph
{
    public abstract class GraphBase : IGraphItem, IInternalGraphHelper
    {
        protected string baseUrl;

        protected IGraphCollection<TChild> CreateGraphCollection<TChild>()
        {
            var collection = new GraphCollection<TChild>();
            collection.Initialize(this);
            return collection;
        }

        protected IGraphItem<TChild> CreateGraphItem<TChild>()
        {
            var collection = new GraphItem<TChild>();
            collection.Initialize(this);
            return collection;
        }

        protected IGraphCollection<TChild> CreateGraphCollection<TChild>(string navigationNodeName)
        {
            var collection = new GraphCollection<TChild>();
            collection.Initialize(this);
            collection.SetFilter(navigationNodeName);   
            return collection;
        }


        string IInternalGraphHelper.BaseUrl
        {
            get
            {
                return baseUrl;
            }
            set
            {
                baseUrl = value;
            }
        }

        public abstract IGraphItem Initialize(IGraphItem parent);
    }
}