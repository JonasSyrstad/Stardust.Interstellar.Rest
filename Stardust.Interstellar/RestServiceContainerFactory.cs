using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public class RestServiceContainerFactory : IServiceContainerFactory
    {
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context) where TService : class
        {

            if (!RestSettings.initialized) RestSettings.Initialize();
            var service = runtime.Context.GetEndpointConfiguration<TService>();
            var serviceEndpoint = service.GetEndpoint(service.ActiveEndpoint);
            var serviceContainer = new RestServiceContainer<TService>(runtime);
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.useAudienceAsServiceRoot", true)) serviceContainer.SetServiceRoot(serviceEndpoint.Audience + "/" + ConfigurationManagerHelper.GetValueOnKey("stardust.useAudienceAsServiceRoot",""));
            else serviceContainer.SetServiceRoot(serviceEndpoint.Address);
            return serviceContainer;
        }
    }
}