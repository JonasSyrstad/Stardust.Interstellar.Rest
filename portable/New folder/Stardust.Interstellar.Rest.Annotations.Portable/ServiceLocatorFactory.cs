using System;
using System.Collections.Generic;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Annotations
{
    public static class ServiceLocatorFactory
    {


        private static IServiceLocator locator;

        private static Type xmlSerializer;

        public static void SetServiceLocator(IServiceLocator serviceLocator)
        {
            locator = serviceLocator;
        }

        internal static IServiceLocator GetLocator()
        {
            return locator;
        }

        public static T GetService<T>()
        {
            return locator != null ? locator.GetService<T>() : default(T);
        }

        public static IEnumerable<T> GetServices<T>()
        {
            return locator?.GetServices<T>();
        }

        
    }
}