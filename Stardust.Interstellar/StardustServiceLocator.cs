using System.Collections.Generic;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    public class StardustServiceLocator : IServiceLocator
    {
        public T GetService<T>()
        {
            return Resolver.Activate<T>();
        }

        public IEnumerable<T> GetServices<T>()
        {
            return Resolver.GetAllInstances<T>();
        }
    }
}