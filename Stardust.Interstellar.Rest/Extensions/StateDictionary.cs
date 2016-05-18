using System;
using System.Collections.Generic;

namespace Stardust.Interstellar.Rest.Extensions
{
    [Serializable]
    public class StateDictionary : Extras
    {

        /// <summary>
        /// Contains additional information passed to the client application 
        /// </summary>
        public Extras Extras => GetState<Extras>("stardust.extras");
    }

    [Serializable]
    public class Extras : Dictionary<string, object>
    {
        public T GetState<T>(string key)
        {
            object state;
            if (!TryGetValue(key, out state)) return default(T);
            if (state != null) return (T)state;
            return default(T);
        }

        public void SetState<T>(string key, T value)
        {
            if (ContainsKey(key)) return;
            Add(key, value);
        }
        
    }
}