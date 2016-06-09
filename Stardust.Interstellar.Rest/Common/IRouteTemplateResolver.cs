using System;
using System.Reflection;

namespace Stardust.Interstellar.Rest.Common
{
    public interface IRouteTemplateResolver
    {
        string GetTemplate(MethodInfo methodInfo);
    }

    public interface ILogger
    {
        void Error(Exception error);

        void Message(string message);

        void Message(string format, params object[] args);
    }
}