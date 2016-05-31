using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public class GraphItem<T> : GraphBase, IGraphItem<T>, IInternalGraphHelper
    {
        public string Id { get; set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public GraphItem(string id)
        {
            if (GetOnLoad)
            {
                Task.Run(async () => { await GetAsync(); }).Wait();
            }
            Id = id;
        }

        protected bool GetOnLoad { get; set; }

        public GraphItem()
        { }

        private IGraphItem parent;

        private object service;

        private T localCopy;

        public override IGraphItem Initialize(IGraphItem parent)
        {
            this.parent = parent;
            InternalBaseUrl = ((IInternalGraphHelper)parent).BaseUrl;
            service = ProxyFactory.CreateInstance(typeof(IGraphCollectionService<T>), ((IInternalGraphHelper)parent).BaseUrl, null);
            return this;
        }

        public async Task<T> GetAsync()
        {
            if (localCopy == null)
            {
                localCopy = await (Task<T>)service.GetType().InvokeMember("GetAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { Id });
                var gb = localCopy as GraphBase;
                gb?.Initialize(this);
            }

            return localCopy;
        }

        public async Task DeleteAsync()
        {
            await (Task)service.GetType().InvokeMember("RemoveAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { Id });
        }

        public async Task SaveAsync()
        {
            await GetAsync();
            await (Task)service.GetType().InvokeMember("UpdateAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { Id, localCopy });
        }

        public T Value
        {
            get
            {
                Task.Run(async () => { await GetAsync(); }).Wait();
                
                return localCopy;
            }
        }
        
    }
}