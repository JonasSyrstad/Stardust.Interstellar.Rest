using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Common;

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
            StateDictionary removedState;
            stateContainer.TryRemove(response.ActionId(), out removedState);
        }

        internal static void EndState(this ResultWrapper response)
        {
            StateDictionary removedState;
            stateContainer.TryRemove(response.ActionId, out removedState);
        }

        public static void InitializeState(this HttpRequestMessage request)
        {
            GetState(request);
        }

        internal static StateDictionary GetState(this HttpRequestMessage request)
        {
            var actionId = request.ActionId();
            if (string.IsNullOrWhiteSpace(actionId))
            {

                actionId = Guid.NewGuid().ToString();
                request.Headers.Add(ExtensionsFactory.ActionIdName, actionId);
            }
            return InitializeState(actionId);
        }

        internal static StateDictionary GetState(this HttpRequestHeaders headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        internal static StateDictionary GetState(this HttpResponseHeaders headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        internal static StateDictionary GetState(this ResultWrapper result)
        {
            var actionId = result.ActionId;
            return InitializeState(actionId);
        }

        private static StateDictionary InitializeState(string actionId)
        {
            StateDictionary state;
            if (!stateContainer.TryGetValue(actionId, out state))
            {
                state = new StateDictionary { { "stardust.extras", new Extras() } };
                stateContainer.TryAdd(actionId, state);
            }
            return state;
        }

        internal static StateDictionary GetState(this HttpWebRequest req)
        {
            var actionId = req.ActionId();
            return InitializeState(actionId);
        }

        internal static void EndState(this HttpRequestMessage request)
        {
            StateDictionary removedState;
            stateContainer.TryRemove(request.ActionId(), out removedState);
        }
        internal static StateDictionary GetState(this HttpWebResponse response)
        {
            var actionId = response.ActionId();
            return InitializeState(actionId);
        }

        private static ConcurrentDictionary<string, StateDictionary> stateContainer = new ConcurrentDictionary<string, StateDictionary>();
    }
}