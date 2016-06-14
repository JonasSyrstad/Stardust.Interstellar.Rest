using System;

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
}