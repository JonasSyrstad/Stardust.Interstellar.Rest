using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Stardust.Interstellar.Rest.Service
{
    public static class ServiceFactory
    {
        static ServiceBuilder Builder = new ServiceBuilder();

        private static List<Type> ServiceTypes=new List<Type>();

        public static Type CreateServiceImplementation<T>()
        {
            var type= Builder.CreateServiceImplementation<T>();
            if(type!=null)
            ServiceTypes.Add(type);
            return type;
        }

        public static IEnumerable<Type> CreateServiceImplementationForAllInCotainingAssembly<T>()
        {
            var assembly = typeof(T).Assembly;
            return CreateServiceImplementations(assembly);
        }

        public static IEnumerable<Type> CreateServiceImplementations(Assembly assembly)
        {
            var types= assembly.GetTypes().Where(t => t.IsInterface).Select(item => Builder.CreateServiceImplementation(item));
            ServiceTypes.AddRange(types.Where( i=>i!=null));
            return types;
        }

        public static void FinalizeRegistration()
        {

            try
            {
                Builder.Save();
            }
            catch (Exception)
            {
                // ignored
            }
            var parent = (IHttpControllerTypeResolver)GlobalConfiguration.Configuration.Services.GetService(typeof(IHttpControllerTypeResolver));
            CustomAssebliesResolver.SetParent(parent);
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new CustomAssebliesResolver());
        }

        public static Assembly GetAssembly()
        {
            return Builder.GetCustomAssembly();
        }

        public static IEnumerable<Type> GetTypes()
        {
            return ServiceTypes;
        }
    }
}

