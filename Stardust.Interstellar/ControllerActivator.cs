using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Stardust.Core.Wcf;
using Stardust.Interstellar.Rest.Common;
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

    class ActionInvoker : ApiControllerActionInvoker
    {
        public override async Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            IStardustContext scope;
            if (!RuntimeFactory.Current.GetStateStorageContainer().TryGetItem("olmscope", out scope)) scope = RequestResponseScopefactory.CreateScope();
            using (scope)
            {
                
                var context = HttpContext.Current;
               
                Logging.DebugMessage("entering invoker for action {0}", actionContext.ActionDescriptor.ActionName);
                AuthorizeRequest(actionContext, context);
                Logging.DebugMessage("Initializing runtime");
                var tracer = RuntimeFactory.Current.InitializeWithConfigSetName(Utilities.Utilities.GetConfigSetName()).SetEnvironment(Utilities.Utilities.GetEnvironment())
                    .SetServiceName(this, Utilities.Utilities.GetServiceName(), actionContext.Request.RequestUri.ToString());
                Logging.DebugMessage("setting current principal");
                var principal = actionContext.Request.GetUserPrincipal() ?? HttpContext.Current.User;
                if (principal != null) RuntimeFactory.Current.SetCurrentPrincipal(principal);
                try
                {
                    Logging.DebugMessage("Action invoked by: {0}", actionContext.Request.Headers.UserAgent);
                    var result = await base.InvokeActionAsync(actionContext, cancellationToken);
                    return result;
                }
                catch (Exception ex)
                {
                    Logging.DebugMessage("Ending action {0} with failure", actionContext.ActionDescriptor.ActionName);
                    ex.Log();
                    return ExtensionsFactory.GetService<Rest.Service.IErrorHandler>().ConvertToErrorResponse(ex, actionContext.Request);
                }
            }
        }

        private void AuthorizeRequest(HttpActionContext actionContext, HttpContext context)
        {
            var auth = ExtensionsFactory.GetService<IRequestAuthenticator>();
            var principal = auth?.AuthorizeRequest(actionContext);
            if(principal==null) return;
            Thread.CurrentPrincipal = principal;
            context.User = principal;
        }
    }

    public interface IRequestAuthenticator
    {
        /// <summary>
        /// Do custom authorization of the inncomming request
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        IPrincipal AuthorizeRequest(HttpActionContext actionContext);
    }

    public class OAuthRequestAuthenticator:IRequestAuthenticator
    {
        /// <summary>
        /// Do custom authorization of the inncomming request
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public IPrincipal AuthorizeRequest(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            
            if (auth!=null && auth.Scheme.Equals("bearer",StringComparison.InvariantCultureIgnoreCase))
            {
                var token = auth.Parameter;
                return ExtensionsFactory.GetService<IOAuthBearerTokenValidator>().Validate(token);
            }
            return null;
        }
    }

    public interface IOAuthBearerTokenValidator
    {
        IPrincipal Validate(string token);
    }
}
