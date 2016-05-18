using System.Collections.Generic;

namespace Stardust.Interstellar.Rest
{
    public interface IHeaderHandlerFactory
    {
        IEnumerable<IHeaderHandler> GetHandlers();
    }
}