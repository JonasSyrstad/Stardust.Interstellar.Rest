using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Stardust.Interstellar.Rest.Extensions
{
    public static class StateHelper
    {
        public static void InitializeState(this HttpWebRequest req)
        {
            GetState(req);
        }

        public static void EndState(this HttpWebResponse response)
        {
            StateDictionary removedState;
            stateContainer.TryRemove(response.ActionId(), out removedState);
        }

        

        public static void InitializeState(this HttpRequestMessage request)
        {
            GetState(request);
        }

        public static StateDictionary GetState(this HttpRequestMessage request)
        {
            var actionId = request.ActionId();
            if (string.IsNullOrWhiteSpace(actionId))
            {

                actionId = Guid.NewGuid().ToString();
                request.Headers.Add(ActionIdName, actionId);
            }
            return InitializeState(actionId);
        }

        public static StateDictionary GetState(this HttpRequestHeaders headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        public static StateDictionary GetState(this HttpResponseHeaders headers)
        {
            var actionId = headers.ActionId();
            return InitializeState(actionId);
        }

        

        public static StateDictionary InitializeState(string actionId)
        {
            StateDictionary state;
            if (!stateContainer.TryGetValue(actionId, out state))
            {
                state = new StateDictionary { { "stardust.extras", new Extras() } };
                stateContainer.TryAdd(actionId, state);
            }
            return state;
        }

        public static StateDictionary GetState(this HttpWebRequest req)
        {
            var actionId = req.ActionId();
            return InitializeState(actionId);
        }

        public static void EndState(this HttpRequestMessage request)
        {
            StateDictionary removedState;
            stateContainer.TryRemove(request.ActionId(), out removedState);
        }
        public static StateDictionary GetState(this HttpWebResponse response)
        {
            var actionId = response.ActionId();
            return InitializeState(actionId);
        }

        public static string ActionId(this HttpWebRequest request)
        {
            return request.Headers[ActionIdName];
        }

        public static string ActionId(this HttpWebResponse response)
        {
            return response.Headers[ActionIdName];
        }

        public static string ActionId(this HttpRequestHeaders headers)
        {
            return headers.Where(h => h.Key == ActionIdName).Select(h => h.Value).FirstOrDefault().FirstOrDefault();
        }

        public static string ActionId(this HttpResponseHeaders headers)
        {
            return headers.Where(h => h.Key == ActionIdName).Select(h => h.Value).FirstOrDefault().FirstOrDefault();
        }

        public static string ActionId(this HttpRequestMessage request)
        {
            if (request.Headers.Contains(ActionIdName))
                return request.Headers.GetValues(ActionIdName).FirstOrDefault();
            return null;
        }

        private static ConcurrentDictionary<string, StateDictionary> stateContainer = new ConcurrentDictionary<string, StateDictionary>();

        private const string ActionIdName = "sd-ActionId";

        public static void EndState(string actionId)
        {
            StateDictionary removedState;
            stateContainer.TryRemove(actionId, out removedState);
        }
    }
}