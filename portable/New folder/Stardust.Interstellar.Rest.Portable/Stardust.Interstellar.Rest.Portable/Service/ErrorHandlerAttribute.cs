using System;

namespace Stardust.Interstellar.Rest.Service
{
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class ErrorHandlerAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Attribute" /> class.</summary>
        public ErrorHandlerAttribute(Type errorHandlerType)
        {
            this.ErrorHandler = (IErrorHandler)Activator.CreateInstance(errorHandlerType);
        }

        public IErrorHandler ErrorHandler { get; set; }
    }
}