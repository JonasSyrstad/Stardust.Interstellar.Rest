using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Stardust.Interstellar.Rest.Extensions
{
    internal static class StateHelper
    {
        internal static void InitializeState(this HttpWebRequest req)
        {
            GetState(req);
        }

        internal static void EndState(this HttpWebResponse response)
        {
            Dictionary<string, object> removedState;
            stateContainer.TryRemove(response.ActionId(), out removedState);
        }

        internal static void EndState(this ResultWrapper response)
        {
            Dictionary<string, object> removedState;
            StateHelper.stateContainer.TryRemove(response.ActionId, out removedState);
        }

        public static void InitializeState(this HttpRequestMessage request)
        {
            GetState(request);
        }

        internal static Dictionary<string, object> GetState(this HttpRequestMessage request)
        {
            var actionId = request.ActionId();
            if (string.IsNullOrWhiteSpace(actionId))
            {

                actionId = Guid.NewGuid().ToString();
                request.Headers.Add(RestWrapper.ActionIdName,actionId);
            }
            return InitializeState(actionId);
        }

        internal static Dictionary<string, object> GetState(this WebHeaderCollection headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        internal static Dictionary<string, object> GetState(this HttpRequestHeaders headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        internal static Dictionary<string, object> GetState(this HttpResponseHeaders headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        public static Dictionary<string, object> GetExtras(this ResultWrapper result)
        {
            var state = GetState(result);
            return Extras(state);
        }

        public static Dictionary<string, object> Extras(this Dictionary<string, object> state)
        {
            return (Dictionary<string, object>)state["stardust.extras"];
        }

        internal static Dictionary<string, object> GetState(this ResultWrapper result)
        {
            var actionId = result.ActionId;
            return InitializeState(actionId);
        }

        private static Dictionary<string, object> InitializeState(string actionId)
        {
            Dictionary<string, object> state;
            if (!stateContainer.TryGetValue(actionId, out state))
            {
                state = new Dictionary<string, object>();
                state.Add("stardust.extras", new Dictionary<string, object>());
                StateHelper.stateContainer.TryAdd(actionId, state);
            }
            return state;
        }

        internal static Dictionary<string, object> GetState(this HttpWebRequest req)
        {
            var actionId = req.ActionId();
            return InitializeState(actionId);
        }

        internal static void EndState(this HttpRequestMessage request)
        {
            Dictionary<string, object> removedState;
            StateHelper.stateContainer.TryRemove(request.ActionId(), out removedState);
        }
        internal static Dictionary<string, object> GetState(this HttpWebResponse response)
        {
            var actionId = response.ActionId();
            return InitializeState(actionId);
        }

        private static ConcurrentDictionary<string, Dictionary<string, object>> stateContainer = new ConcurrentDictionary<string, Dictionary<string, object>>();
    }
}