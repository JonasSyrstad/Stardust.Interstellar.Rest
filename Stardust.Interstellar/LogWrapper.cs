using System;
using Stardust.Interstellar.Rest.Common;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public class LogWrapper : ILogger
    {
        public void Error(Exception error)
        {
            error.Log("rest generator");
        }

        public void Message(string message)
        {
            Logging.DebugMessage(message);
        }

        public void Message(string format, params object[] args)
        {
            Logging.DebugMessage(format,args);
        }
    }
}