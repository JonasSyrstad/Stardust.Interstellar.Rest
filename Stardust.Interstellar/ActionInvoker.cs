using System;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using Stardust.Core.Wcf;
using Stardust.Interstellar.Rest.Common;
using Stardust.Particles;

namespace Stardust.Interstellar
{
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
}