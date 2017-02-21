﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace Stardust.Interstellar.Rest.Extensions
{
    public static class StateHelper
    {
        static StateHelper()
        {
            if (AutoCleaning)
                PeriodicTask.Run(CleanStateCache, null, TimeSpan.FromMinutes(1), CancellationToken.None);
        }

        public static bool AutoCleaning { get; set; }

        public static long StateContainerSize => stateContainer.Count;

        private static void CleanStateCache(object o, CancellationToken cancellationToken)
        {
            CleanStateStore(10);
        }

        /// <summary>
        /// Cleans the state store by removing all entries older than n minutes.
        /// </summary>
        /// <param name="maxAge">The maximum age in minutes.</param>
        public static void CleanStateStore(double maxAge)
        {
            try
            {
                foreach (var c in stateContainer.Where(i => i.Value.Created < DateTime.UtcNow.AddMinutes(maxAge * -1)).ToArray())
                {
                    LowPriorityContainer deprecatedItem;
                    stateContainer.TryRemove(c.Key, out deprecatedItem);
                }
            }
            catch
            {
            }
        }

        public static void InitializeState(this HttpWebRequest req)
        {
            GetState(req);
        }

        public static void EndState(this HttpWebResponse response)
        {
            LowPriorityContainer removedState;
            stateContainer.TryRemove(response.ActionId(), out removedState);
            removedState.StateReference.Clear();
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
            if (!request.Properties.ContainsKey(ActionIdName))
                request.Properties.Add(ActionIdName, actionId);
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
            LowPriorityContainer state;
            if (!stateContainer.TryGetValue(actionId, out state))
            {
                state = new LowPriorityContainer
                {
                    StateReference = new StateDictionary { { StateDictionary.StardustExtras, new Extras() } }
                };
                stateContainer.TryAdd(actionId, state);
            }
            return state.StateReference;
        }

        public static StateDictionary GetState(this HttpWebRequest req)
        {
            var actionId = req.ActionId();
            return InitializeState(actionId);
        }

        public static void EndState(this HttpRequestMessage request)
        {
            LowPriorityContainer removedState;
            if (!request.Properties.ContainsKey(ActionIdName)) return;
            stateContainer.TryRemove(request.ActionId(), out removedState);
            removedState.StateReference.Clear();
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

        private static ConcurrentDictionary<string, LowPriorityContainer> stateContainer = new ConcurrentDictionary<string, LowPriorityContainer>();

        private const string ActionIdName = "sd-ActionId";

        public static void EndState(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId)) return;
            LowPriorityContainer removedState;
            stateContainer.TryRemove(actionId, out removedState);
            removedState.StateReference.Clear();
        }
    }
}