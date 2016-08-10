using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public class GraphCollection<T> : GraphBase, IGraphCollection<T>, IInternalGraphHelper
    {
        private object service;
        private Type type;

        private IGraphItem parent;

        private string baseUrl;

        private string navigationPropertyName;

        internal GraphCollection(NavigationAttribute navigation)
        {
            baseUrl = ConfigurationManager.AppSettings[navigation.ServiceRootKey];
            service = ProxyFactory.CreateInstance(navigation.AccessorServiceInterface, baseUrl, null);
        }

        public GraphCollection(Type serviceType, string serviceRootKey)
        {
            baseUrl = ConfigurationManager.AppSettings[serviceRootKey];
            service = ProxyFactory.CreateInstance(serviceType, baseUrl, null);
        }

        public GraphCollection(Type serviceType)
        {
            baseUrl = ConfigurationManager.AppSettings["stardust.graphServiceRoot"];
            service = ProxyFactory.CreateInstance(serviceType, baseUrl, base.extrasHandler);
        }

        public GraphCollection()
        {
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            Task<IEnumerable<T>> result;
            if(string.IsNullOrWhiteSpace(navigationPropertyName))
            result = (Task<IEnumerable<T>>)service.GetType().InvokeMember("GetAllAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { });
            else
            {
                result = (Task<IEnumerable<T>>)service.GetType().InvokeMember("GetGraphNodesAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] {Id,navigationPropertyName });
            }
            return Task.Run(async () => await result).Result.Select(
                i =>
                    {
                        SetParent(i);
                        return i;
                    }).GetEnumerator();
        }

        private T SetParent(T i)
        {
            var gi = i as GraphBase;
            gi?.Initialize(this);
            return i;
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[string id]
        {
            get
            {
                var res = (Task<T>)service.GetType().InvokeMember("GetAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { id });
                var item= Task.Run(async () => await res).Result;
                SetParent(item);
                return item;
            }
            set
            {
                SetParent(value);
                var task = (Task)service.GetType().InvokeMember("GetAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { id });
                Task.Run(async () => await task).Wait();
            }
        }

        public async Task AddAsync(T item)
        {
            SetParent(item);
            await (Task)service.GetType().InvokeMember("AddAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { item });
        }


        public async Task DeleteAsync(string id)
        {
            await (Task)service.GetType().InvokeMember("RemoveAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { id });
        }

        public async Task<T> GetAsync(string id)
        {
            var item= await (Task<T>)service.GetType().InvokeMember("GetAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { id });
            return SetParent(item);
        }

        public async Task UpdateAsync(string id, T item)
        {
            SetParent(item);
            await (Task)service.GetType().InvokeMember("UpdateAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { id, item });
        }

        public async Task<IEnumerable<T>> QueryAsync(GraphQuery query)
        {
             var result= await (Task<IEnumerable<T>>)service.GetType().InvokeMember("QueryAsync", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, service, new object[] { query });
            return result.Select(SetParent);
        }

        public override IGraphItem Initialize(IGraphItem parent)
        {
            this.parent = parent;
            InternalBaseUrl = ((IInternalGraphHelper)parent).BaseUrl;
            service = ProxyFactory.CreateInstance(typeof(IGraphCollectionService<T>), ((IInternalGraphHelper)parent).BaseUrl, null);
            return this;
        }

        public void SetFilter(string navigationNodeName,string id)
        {
            Id = id;
            navigationPropertyName = navigationNodeName;
        }

        private string Id { get; set; }
    }
}