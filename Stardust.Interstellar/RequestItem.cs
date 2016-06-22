using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar
{
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