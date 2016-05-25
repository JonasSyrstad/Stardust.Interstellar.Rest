using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Stardust.Core.Service.Web;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Interstellar.Trace;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public class StardustHeaderHandler : StatefullHeaderHandlerBase
    {
        private readonly IRuntime runtime;

        private readonly ISupportCodeGenerator generator;

        public StardustHeaderHandler(IRuntime runtime, ISupportCodeGenerator generator)
        {
            this.runtime = runtime;
            this.generator = generator;
        }

        protected override void DoSetHeader(StateDictionary state, HttpWebRequest req)
        {
            var tracer = TracerFactory.StartTracer(this, req.RequestUri.ToString());
            state.Add("tracer",tracer);
            var respHeader = new RequestHeader
                                 {
                                     ReferingMessageId = runtime.RequestContext?.RequestHeader?.MessageId,
                                     RuntimeInstance = runtime.InstanceId.ToString(),
                                     MessageId = state.GetState<Guid>("messageId").ToString(),
                                     ServerIdentity = Environment.MachineName,
                                     SupportCode = runtime.RequestContext?.RequestHeader?.SupportCode,
                                     TimeStamp = DateTime.UtcNow,
                                     Environment = runtime.Environment,
                                     ConfigSet = Utilities.Utilities.GetConfigSetName(),
                                     MethodName = req.RequestUri.ToString(),
                                     ServiceName = Utilities.Utilities.GetServiceName()
                                 };
            req.Headers.Add("x-stardustMeta", Convert.ToBase64String(JsonConvert.SerializeObject(respHeader).GetByteArray()));
        }

        protected override void DoGetHeader(StateDictionary state, HttpWebResponse response)
        {
            var meta = response.Headers["x-stardustMeta"];
            if (meta!=null)
            {
                var item = JsonConvert.DeserializeObject<ResponseHeader>(Convert.FromBase64String(meta).GetStringFromArray());
                state.Extras.SetState("x-stardustMeta",item);
                if(state.GetState<ITracer>("tracer").GetCallstack().CallStack==null) state.GetState<ITracer>("tracer").GetCallstack().CallStack=new List<CallStackItem>();
                state.GetState<ITracer>("tracer").GetCallstack().CallStack.Add(item.CallStack);
            }
            state.GetState<ITracer>("tracer").TryDispose();
            
        }

        protected override void DoSetServiceHeaders(StateDictionary state, HttpResponseHeaders headers)
        {
            runtime.TearDown("");
            var respHeader = new ResponseHeader
                                 {
                                     ReferingMessageId = runtime.RequestContext?.RequestHeader?.MessageId,
                                     OriginalRuntimeInstance = runtime.RequestContext?.RequestHeader?.RuntimeInstance,
                                     RuntimeInstance = runtime.InstanceId.ToString(),
                                     CallStack = runtime.CallStack,
                                     ExecutionTime = (long)(runtime.CallStack.ExecutionTime.HasValue ? runtime.CallStack.ExecutionTime.Value : -1),
                                     MessageId = state.GetState<Guid>("messageId").ToString(),
                                     ServerIdentity = Environment.MachineName,
                                     SupportCode = runtime.RequestContext?.RequestHeader?.SupportCode,
                                     TimeStamp = DateTime.UtcNow

                                 };
            headers.Add("x-stardustMeta", Convert.ToBase64String(JsonConvert.SerializeObject(respHeader).GetByteArray()));
            headers.Add(SupportCodeHeaderName,state.GetState<string>(SupportCodeHeaderName));
        }

        protected override void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers)
        {
            InitializeStardustRuntime(headers,state);
            if (headers.Contains("x-stardustMeta"))
            {
                var item = JsonConvert.DeserializeObject<RequestHeader>(Convert.FromBase64String(headers.GetValues("x-stardustMeta").First()).GetStringFromArray());
                state.Add("RequestHeadder", item);
                runtime.InitializeWithMessageContext(new RequestItem(item));
            }
        }

        private void InitializeStardustRuntime(HttpRequestHeaders headers, StateDictionary state)
        {
            runtime.SetEnvironment(Utilities.Utilities.GetEnvironment());
            var tracer= runtime.SetServiceName(state.GetState<ApiController>("controller"), Utilities.Utilities.GetServiceName(), state.GetState<string>("action"));
            tracer.GetCallstack().Name = state.GetState<string>("controllerName");
            runtime.SetCurrentPrincipal(HttpContext.Current.User);
            var supportCode = CreateSupportCode();
            state.SetState(SupportCodeHeaderName,supportCode);
            runtime.TrySetSupportCode(supportCode);
            runtime.GetStateStorageContainer().TryAddStorageItem(HttpContext.Current, "httpContext");
        }
        private string CreateSupportCode()
        {
            try
            {
                var supportCode = GetSupportCodeFromHeader();
                if (supportCode.ContainsCharacters())
                {
                    return supportCode;
                }
                if (generator != null)
                {
                    return generator.CreateSupportCode();
                }
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                ex.Log();
                return Guid.NewGuid().ToString();
            }
        }
        private static string GetSupportCodeFromHeader()
        {
            try
            {
                var val = HttpContext.Current.Request.Headers[SupportCodeHeaderName];
                return val;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        internal const string SupportCodeHeaderName = "x-supportCode";
    }

    

    public class RequestItem : IRequestBase
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public RequestItem()
        {
        }

        public RequestItem(RequestHeader item)
        {
            RequestHeader = item;
        }

        public RequestHeader RequestHeader { get; set; }
    }
}