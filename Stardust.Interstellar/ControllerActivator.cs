using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Stardust.Core.Wcf;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    class ControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            var scope = RequestResponseScopefactory.CreateScope();
            var runtime = RuntimeFactory.CreateRuntime();
            if(request.Headers.Contains("x-supportCode"))
            {
                var supportCode = request.Headers.GetValues("x-supportCode")?.FirstOrDefault();
                if (supportCode.ContainsCharacters()) RuntimeFactory.Current.TrySetSupportCode(supportCode);
            }
            runtime.GetStateStorageContainer().TryAddStorageItem(scope, "olmscope");
            var controller = (IHttpController)ActivatorFactory.Activator.Activate(controllerType);
            return controller;
        }
    }
}
