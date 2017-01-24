using System;

namespace Stardust.Interstellar.Rest.Extensions
{
    [Serializable]
    public class StateDictionary : Extras
    {
        public const string StardustExtras = "stardust.extras";

        /// <summary>
        /// Contains additional information passed to the client application 
        /// </summary>
        public Extras Extras => GetState<Extras>(StardustExtras);
    }
}