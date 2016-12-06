﻿using System;
using System.Net;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;

namespace Stardust.Interstellar.Rest.Client.CircuitBreaker
{
    public class CircuitBreaker : ICircuitBreaker, ICircuit
    {
        public CircuitBreaker(int threshold, TimeSpan timeout, TimeSpan resetTimeout, Type[] ignoredExceptions, HttpStatusCode[] ignoredStatusCodes, Type circuitBrakerMonitor)
        {
            if (threshold < 1)
            {
                throw new ArgumentOutOfRangeException("threshold", "Threshold should be greater than 0");
            }

            if (timeout.TotalMilliseconds < 1)
            {
                throw new ArgumentOutOfRangeException("timeout", "Timeout should be greater than 0");
            }

            Threshold = threshold;
            Timeout = timeout;
            ResetTimeout = resetTimeout;
            IgnoredExceptions = ignoredExceptions;
            IgnoredStatusCodes = ignoredStatusCodes;
            MoveToClosedState();
            if (circuitBrakerMonitor != null)
                monitor = (ICircuitBreakerMonitor)Activator.CreateInstance(circuitBrakerMonitor);
        }


        private readonly object triowing = new object();
        private CircuitBreakerStateBase state;
        private Exception exceptionFromLastAttemptCall;
        private ICircuitBreakerMonitor monitor;

        public CircuitBreaker(CircuitBreakerAttribute attribute) : this(attribute.Threshold, attribute.Timeout, attribute.ResetTimeout, attribute.IgnoredExceptionTypes, attribute.IgnoredStatusCodes, attribute.Monitor)
        {
        }

        public int Failures { get; private set; }

        DateTime? ICircuit.LastFailure => LastErrorTime;

        TimeSpan? ICircuit.SuspendedTime => ((LastErrorTime + ResetTimeout) - DateTime.UtcNow);

        Exception ICircuit.LastError => GetExceptionFromLastAttemptCall();

        public int Threshold { get; }

        public TimeSpan Timeout { get; }

        public TimeSpan ResetTimeout { get; private set; }

        public Type[] IgnoredExceptions { get; private set; }

        public bool IsClosed
        {
            get { return state.Update() is ClosedState; }
        }

        public bool IsOpen
        {
            get { return state.Update() is OpenState; }
        }

        public bool IsHalfOpen
        {
            get { return state.Update() is HalfOpenState; }
        }

        public HttpStatusCode[] IgnoredStatusCodes { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public string ServiceName { get; set; }

        public ICircuitBreakerMonitor Monitor
        {
            get
            {
                if (monitor == null)
                    monitor = ExtensionsFactory.GetService<ICircuitBreakerMonitor>() ?? new NullBreaker();
                return monitor;

            }
        }

        internal CircuitBreakerStateBase MoveToClosedState()
        {
            state = new ClosedState(this);
            return state;
        }

        internal CircuitBreakerStateBase MoveToOpenState()
        {
            state = new OpenState(this);
            return state;
        }

        internal CircuitBreakerStateBase MoveToHalfOpenState()
        {
            state = new HalfOpenState(this);
            return state;
        }

        public Exception GetExceptionFromLastAttemptCall()
        {
            return exceptionFromLastAttemptCall;
        }

        public virtual ResultWrapper Execute(string path, Func<ResultWrapper> func)
        {
            ResultWrapper result;
            if (PreCallProcessing(path, out result)) return result;
            try
            {
                result = func();
                if (result.Error != null)
                {
                    PostProcessing(path, result);
                    return result;
                }
            }
            catch (Exception e)
            {
                return PostProcessing(path, e);
            }

            PostProcessing();
            return result;
        }

        public virtual async Task<ResultWrapper> ExecuteAsync(string path, Func<Task<ResultWrapper>> func)
        {
            ResultWrapper result;
            if (PreCallProcessing(path, out result)) return result;
            try
            {
                result = await func();
                if (result.Error != null)
                {
                    PostProcessing(path, result);
                    return result;
                }
            }
            catch (Exception e)
            {
                return PostProcessing(path, e);
            }


            PostProcessing();
            return result;
        }

        public T Invoke<T>(string actionUrl, Func<T> func)
        {
            T result;
            PreCallProcessing(actionUrl);
            try
            {
                result = func();

            }
            catch (Exception e)
            {
                this.exceptionFromLastAttemptCall = e;
                lock (triowing)
                {
                    state.ActUponException(actionUrl, e);
                }
                throw;
            }

            PostProcessing();
            return result;
        }


        public async Task<T> InvokeAsync<T>(string actionUrl, Func<Task<T>> func)
        {
            T result;
            PreCallProcessing(actionUrl);
            try
            {
                result = await func();

            }
            catch (Exception e)
            {
                this.exceptionFromLastAttemptCall = e;
                lock (triowing)
                {
                    state.ActUponException(actionUrl, e);
                }
                throw;
            }

            PostProcessing();
            return result;
        }

        public void Invoke(string actionUrl, Action func)
        {
            
            PreCallProcessing(actionUrl);
            try
            {
                func();

            }
            catch (Exception e)
            {
                this.exceptionFromLastAttemptCall = e;
                lock (triowing)
                {
                    state.ActUponException(actionUrl, e);
                }
                throw;
            }

            PostProcessing();
        }

        public async Task InvokeAsync(string actionUrl, Func<Task> func)
        {
            PreCallProcessing(actionUrl);
            try
            {
                await func();

            }
            catch (Exception e)
            {
                this.exceptionFromLastAttemptCall = e;
                lock (triowing)
                {
                    state.ActUponException(actionUrl, e);
                }
                throw;
            }

            PostProcessing();
        }

        private bool PreCallProcessing(string actionUrl)
        {
            lock (triowing)
            {
                state.ProtectedCodeIsAboutToBeCalled();
                if (state is OpenState)
                {
                    var suspended = ((LastErrorTime + ResetTimeout) - DateTime.UtcNow);
                    throw new SuspendedDependencyException(
                        $"Calls to {actionUrl} is suspended for {suspended?.TotalMinutes ?? 0} minutes",
                        exceptionFromLastAttemptCall);
                }
            }
            this.exceptionFromLastAttemptCall = null;
            return false;
        }

        private ResultWrapper PostProcessing(string path, Exception e)
        {
            this.exceptionFromLastAttemptCall = e;
            lock (triowing)
            {
                state.ActUponException(path, e);
            }
            return new ResultWrapper { Error = e, ActionId = Guid.NewGuid().ToString() };
        }

        private void PostProcessing(string path, ResultWrapper result)
        {
            exceptionFromLastAttemptCall = result.Error;
            lock (triowing)
            {
                state.ActUponException(path, result.Error);
            }
        }

        private void PostProcessing()
        {
            lock (triowing)
            {
                state.ProtectedCodeHasBeenCalled();
            }
        }

        private bool PreCallProcessing(string path, out ResultWrapper execute)
        {
            ResultWrapper result;
            lock (triowing)
            {
                state.ProtectedCodeIsAboutToBeCalled();
                if (state is OpenState)
                {
                    {
                        var suspended = ((LastErrorTime + ResetTimeout) - DateTime.UtcNow);
                        execute = new ResultWrapper
                        {
                            Error = new SuspendedDependencyException($"Calls to {path} is suspended for {suspended?.TotalMinutes ?? 0} minutes", exceptionFromLastAttemptCall),
                            Status = HttpStatusCode.TemporaryRedirect,
                            StatusMessage = $"Calls to {path} is suspended for {suspended?.TotalMinutes ?? 0d} minutes",
                            ActionId = Guid.NewGuid().ToString()
                        };
                        execute.GetState().Extras.Add("x-serviceSuspended", $"{suspended?.TotalMinutes ?? 0d} minutes");
                        execute.GetState().Extras.Add("x-suspendedService", path);
                        return true;
                    }
                }
            }
            this.exceptionFromLastAttemptCall = null;
            execute = null;
            return false;
        }

        public void IncreaseFailureCount()
        {
            Failures++;
            LastErrorTime = DateTime.UtcNow;
        }

        public void ResetFailureCount()
        {
            Failures = 0;
            LastErrorTime = null;
        }

        public bool IsThresholdReached()
        {
            return Failures >= Threshold;
        }
    }
}