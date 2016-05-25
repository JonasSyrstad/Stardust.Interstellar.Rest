using System;
using System.IdentityModel.Tokens;
using System.Net;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    public class RestServiceContainer<T> : IServiceContainer<T>
    {
        private readonly IRuntime runtime;

        private string serviceRoot;

        private T client;

        public RestServiceContainer(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {

        }

        public TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase
        {
            throw new NotImplementedException();
        }

        public IServiceContainer<T> Initialize(bool useSecure = false)
        {
            return this;
        }

        public IServiceContainer<T> SetCredentials(string username, string password)
        {
            var autHandler = Resolver.Activate<IAuthenticationHandler>() as AuthHandler;
            autHandler.SetNetworkCredentials(new NetworkCredential(username, password));
            return this;
        }

        public T GetClient()
        {
            if (client == null)
            {
                client = ProxyFactory.CreateInstance<T>(
                    serviceRoot,
                    RestSettings.ExtrasHandler);

            }
            return client;
        }

        public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
        {
            return this;
        }

        public string GetUrl()
        {
            return serviceRoot;
        }

        public bool Initialized { get; }

        public void SetNettworkCredentials(NetworkCredential credential)
        {
            var autHandler = Resolver.Activate<IAuthenticationHandler>() as AuthHandler;
            autHandler.SetNetworkCredentials(credential);
        }

        public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
        {
            return this;
        }
    }
}