using System;
using System.Net;

namespace Stardust.Interstellar.Rest.Annotations
{
    /// <summary>
    /// Apply the Circuit breaker pattern to the service client
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class CircuitBreakerAttribute : Attribute
    {
        private Type _monitor;

        public CircuitBreakerAttribute(int threshold, int timeoutInMinutes, Type[] ignoredExceptionTypes, HttpStatusCode[] ignoredStatusCodes) : this(threshold, timeoutInMinutes, timeoutInMinutes, ignoredExceptionTypes, ignoredStatusCodes)
        { }


        public CircuitBreakerAttribute(int threshold, int timeoutInMinutes) : this(threshold, timeoutInMinutes, timeoutInMinutes)
        { }

        public CircuitBreakerAttribute(int threshold, int timeoutInMinutes, int resetTimeout, Type[] ignoredExceptionTypes, HttpStatusCode[] ignoredStatusCodes)
        {
            Threshold = threshold;
            Timeout = TimeSpan.FromMinutes(timeoutInMinutes);
            IgnoredExceptionTypes = ignoredExceptionTypes;
            IgnoredStatusCodes = ignoredStatusCodes;
            ResetTimeout = TimeSpan.FromMinutes(resetTimeout);
        }


        public CircuitBreakerAttribute(int threshold, int timeoutInMinutes, int resetTimeout)
        {
            Threshold = threshold;
            Timeout = TimeSpan.FromMinutes(timeoutInMinutes);
            ResetTimeout = TimeSpan.FromMinutes(resetTimeout);
            IgnoredExceptionTypes = new[] { typeof(UnauthorizedAccessException), typeof(NullReferenceException) };
            IgnoredStatusCodes = new[]
            {
                HttpStatusCode.Forbidden,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.PreconditionFailed,
                HttpStatusCode.Ambiguous
            };
        }
        public int Threshold { get; set; }
        public TimeSpan Timeout { get; set; }
        public Type[] IgnoredExceptionTypes { get; set; }
        public HttpStatusCode[] IgnoredStatusCodes { get; set; }
        public TimeSpan ResetTimeout { get; set; }

        public Type Monitor
        {
            get { return _monitor; }
            set
            {
                if (!typeof(ICircuitBreakerMonitor).IsAssignableFrom(value))
                    throw new InvalidCastException($"Unable to assign {value.FullName} to {typeof(ICircuitBreakerMonitor).FullName}");
                _monitor = value;
            }
        }
    }

    public interface ICircuitBreakerMonitor
    {
        /// <summary>
        /// The Circuit Breaker is opened. Add notifications and proactive efforts here
        /// </summary>
        /// <param name="circuitBreakerServiceName"></param>
        /// <param name="exception"></param>
        /// <param name="state"></param>
        void Trip(string circuitBreakerServiceName, Exception exception, ICircuitBreakerState state);

        /// <summary>
        /// Programatically determine if an exception is a part of the application flow of control or if it is broken and should be suspended
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool IsExceptionIgnorable(Exception exception);
    }

    public interface ICircuitBreakerState
    {
        ICircuit Circuit { get; }

        string State { get; }
    }

    public interface ICircuit
    {
        int Failures { get; }

        DateTime? LastFailure { get; }

        TimeSpan? SuspendedTime { get; }

        Exception LastError { get; }
    }
}