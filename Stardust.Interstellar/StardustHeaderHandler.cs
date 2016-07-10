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
using Stardust.Interstellar.Trace;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public class StardustHeaderHandler : StatefullHeaderHandlerBase
    {

        private readonly ISupportCodeGenerator generator;

        public StardustHeaderHandler( ISupportCodeGenerator generator)
        {
            this.generator = generator;
        }

        /// <summary>
        /// The order of execution. Lower numbers will be processed first
        /// </summary>
        public override int ProcessingOrder => 0;

        protected override void DoSetHeader(StateDictionary state, HttpWebRequest req)
        {
            var runtime = RuntimeFactory.Current;
            var tracer = TracerFactory.StartTracer(this, req.RequestUri.ToString());
            state.Add("tracer", tracer);
            string supportCode = null;
            runtime.GetStateStorageContainer().TryGetItem<string>("supportCode", out supportCode);
            if (supportCode.IsNullOrWhiteSpace())
                supportCode = runtime.RequestContext?.RequestHeader?.SupportCode;
            else
            {
                if (runtime.RequestContext?.RequestHeader != null)
                    runtime.RequestContext.RequestHeader.SupportCode = supportCode;
            }

            if (supportCode.ContainsCharacters())
                req.Headers.Set(SupportCodeHeaderName, supportCode);
            var respHeader = new RequestHeader
            {
                ReferingMessageId = runtime.RequestContext?.RequestHeader?.MessageId,
                RuntimeInstance = runtime.InstanceId.ToString(),
                MessageId = state.GetState<Guid>(Messageid).ToString(),
                ServerIdentity = Environment.MachineName,
                SupportCode = supportCode,
                TimeStamp = DateTime.UtcNow,
                Environment = runtime.Environment,
                ConfigSet = Utilities.Utilities.GetConfigSetName(),
                MethodName = req.RequestUri.ToString(),
                ServiceName = Utilities.Utilities.GetServiceName(),
                ConfigVersion = runtime.Context.GetEnvironmentConfiguration().Version
            };

            req.Headers.Add(StardustMetadataName, Convert.ToBase64String(JsonConvert.SerializeObject(respHeader).GetByteArray()));
        }

        protected override void DoGetHeader(StateDictionary state, HttpWebResponse response)
        {
            var meta = response.Headers[StardustMetadataName];
            if (meta != null)
            {
                var item = JsonConvert.DeserializeObject<ResponseHeader>(Convert.FromBase64String(meta).GetStringFromArray());
                state.Extras.SetState(StardustMetadataName, item);
                if (state.GetState<ITracer>("tracer").GetCallstack().CallStack == null) state.GetState<ITracer>("tracer").GetCallstack().CallStack = new List<CallStackItem>();
                state.GetState<ITracer>("tracer").GetCallstack().CallStack.Add(item.CallStack);
            }
            state.GetState<ITracer>("tracer").TryDispose();

        }

        protected override void DoSetServiceHeaders(StateDictionary state, HttpResponseHeaders headers)
        {
            var runtime = RuntimeFactory.Current;
            runtime.TearDown("");

            
            var respHeader = new ResponseHeader
            {
                ReferingMessageId = runtime.RequestContext?.RequestHeader?.MessageId,
                OriginalRuntimeInstance = runtime.RequestContext?.RequestHeader?.RuntimeInstance,
                RuntimeInstance = runtime.InstanceId.ToString(),
                CallStack = runtime.CallStack,
                ExecutionTime = (long)(runtime.CallStack!=null&& runtime.CallStack.ExecutionTime.HasValue ? runtime.CallStack.ExecutionTime.Value : -1),
                MessageId = state.GetState<Guid>(Messageid).ToString(),
                ServerIdentity = Environment.MachineName,
                SupportCode = runtime.RequestContext?.RequestHeader?.SupportCode,
                TimeStamp = DateTime.UtcNow,
                ConfigVersion = runtime.Context.GetEnvironmentConfiguration().Version
            };
            headers.Add(StardustMetadataName, Convert.ToBase64String(JsonConvert.SerializeObject(respHeader).GetByteArray()));
            headers.Add(SupportCodeHeaderName, state.GetState<string>(SupportCodeHeaderName));
        }

        protected override void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers)
        {
            var runtime = RuntimeFactory.Current;
            InitializeStardustRuntime(headers, state);

            if (headers.Contains(StardustMetadataName))
            {
                var item = JsonConvert.DeserializeObject<RequestHeader>(Convert.FromBase64String(headers.GetValues(StardustMetadataName).First()).GetStringFromArray());
                state.Add("RequestHeader", item);
                runtime.InitializeWithMessageContext(new RequestItem(item));
                state.SetState(Messageid, Guid.NewGuid());
            }
            if (string.IsNullOrWhiteSpace(runtime.RequestContext?.RequestHeader?.SupportCode)) return;
            if (headers.Contains(SupportCodeHeaderName))
                runtime.TrySetSupportCode(headers.GetValues(SupportCodeHeaderName).First());
        }

        private void InitializeStardustRuntime(HttpRequestHeaders headers, StateDictionary state)
        {
            var runtime = RuntimeFactory.Current;
            runtime.SetEnvironment(Utilities.Utilities.GetEnvironment());
            var tracer = runtime.SetServiceName(state.GetState<ApiController>("controller"), Utilities.Utilities.GetServiceName(), state.GetState<string>("action"));
            tracer.GetCallstack().Name = state.GetState<string>("controllerName");
            runtime.SetCurrentPrincipal(HttpContext.Current.User);
            string supportCode = null;
            if (headers.Contains(SupportCodeHeaderName))
                supportCode = headers.GetValues(SupportCodeHeaderName).FirstOrDefault();
            if (supportCode.IsNullOrWhiteSpace()) runtime.GetStateStorageContainer().TryGetItem(SupportCodeHeaderName, out supportCode);
            if (supportCode.IsNullOrWhiteSpace())
                supportCode = CreateSupportCode();
            state.SetState(SupportCodeHeaderName, supportCode);
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

        private const string SupportCodeHeaderName = "x-supportCode";

        private const string StardustMetadataName = "x-stardustMeta";

        private const string Messageid = "messageId";
    }
}