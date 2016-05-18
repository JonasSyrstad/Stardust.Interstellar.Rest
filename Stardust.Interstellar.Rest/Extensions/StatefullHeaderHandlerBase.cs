using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Extensions
{
    public abstract class StatefullHeaderHandlerBase:IHeaderHandler
    {
        internal static void InitializeState(HttpWebRequest req)
        {
            GetState(req);
        }


        internal static void EndState(HttpWebResponse response)
        {
            Dictionary<string, object> removedState;
            stateContainer.TryRemove(response.ActionId(), out removedState);
        }

        private static ConcurrentDictionary<string, Dictionary<string, object>> stateContainer = new ConcurrentDictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// Set custom header values on sending request to a service
        /// </summary>
        /// <param name="req"></param>
        public void SetHeader(HttpWebRequest req)
        {
            var state = GetState(req);
            DoSetHeader(state, req);
        }

        private static Dictionary<string, object> GetState(HttpWebRequest req)
        {
            Dictionary<string, object> state;
            if (!stateContainer.TryGetValue(req.ActionId(), out state))
            {
                state = new Dictionary<string, object>();
                stateContainer.TryAdd(req.ActionId(), state);
            }
            return state;
        }

        protected abstract void DoSetHeader(Dictionary<string, object> state, HttpWebRequest req);

        /// <summary>
        /// Get header values form a service response
        /// </summary>
        /// <param name="response"></param>
        public void GetHeader(HttpWebResponse response)
        {
            var state = GetState(response);
            DoGetHeader(state, response);
        }

        private Dictionary<string, object> GetState(HttpWebResponse response)
        {
            Dictionary<string, object> state;
            if (!stateContainer.TryGetValue(response.ActionId(), out state))
            {
                state = new Dictionary<string, object>();
                stateContainer.TryAdd(response.ActionId(), state);
            }
            return state;
        }

        protected abstract void DoGetHeader(Dictionary<string, object> state, HttpWebResponse response);

        internal static void EndState(HttpRequestMessage request)
        {
            Dictionary<string, object> removedState;
            stateContainer.TryRemove(request.ActionId(), out removedState);
        }

        /// <summary>
        /// Set custom header values before sending the response from the service
        /// </summary>
        /// <param name="headers"></param>
        public void SetServiceHeaders(WebHeaderCollection headers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get custom header values received from the client 
        /// </summary>
        /// <param name="headers"></param>
        public void GetServiceHeader(HttpRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        internal static void EndState(ResultWrapper response)
        {
            Dictionary<string, object> removedState;
            stateContainer.TryRemove(response.ActionId, out removedState);
        }

        public static void InitializeState(HttpRequestMessage request)
        {
            GetState(request);
        }

        private static Dictionary<string, object>  GetState(HttpRequestMessage request)
        {
            Dictionary<string, object> state;
            if (!stateContainer.TryGetValue(request.ActionId(), out state))
            {
                state = new Dictionary<string, object>();
                stateContainer.TryAdd(request.ActionId(), state);
            }
            return state;
        }
    }
}
