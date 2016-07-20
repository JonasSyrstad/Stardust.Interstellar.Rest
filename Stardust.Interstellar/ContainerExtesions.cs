using Stardust.Interstellar.Rest.Client;

namespace Stardust.Interstellar
{
    public static class ContainerExtesions
    {
        public static IServiceContainer<T> SetHeaderValue<T>(this IServiceContainer<T> container, string name, string value)
        {
            var wrapper = container.GetClient() as RestWrapper;
            wrapper?.SetHttpHeader(name, value);
            return container;
        }
    }
}