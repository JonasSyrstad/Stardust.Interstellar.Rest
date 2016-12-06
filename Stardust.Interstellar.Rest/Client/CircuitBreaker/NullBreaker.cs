﻿using System;
using System.Net;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;

namespace Stardust.Interstellar.Rest.Client.CircuitBreaker
{
    internal class NullBreaker : ICircuitBreaker, ICircuitBreakerMonitor
    {
        public ResultWrapper Execute(string path, Func<ResultWrapper> func)
        {
            return func();
        }

        public async Task<ResultWrapper> ExecuteAsync(string path,Func<Task<ResultWrapper>> func)
        {
            return await func();
        }

        public T Invoke<T>(string actionUrl, Func<T> func)
        {
            return func();
        }

        public async Task<T> InvokeAsync<T>(string actionUrl, Func<Task<T>> func)
        {
            return await func();
        }

        public void Invoke(string actionUrl, Action func)
        {
            func();
        }

        public async Task InvokeAsync(string actionUrl, Func<Task> func)
        {
            await func();
        }

        public void Trip(string circuitBreakerServiceName, Exception exception, ICircuitBreakerState state)
        {
            ExtensionsFactory.GetService<ILogger>()?.Error(exception);
            var webEx = exception as WebException ?? (exception as AggregateException)?.InnerException as WebException;
            ExtensionsFactory.GetService<ILogger>()?.Message($"Invocation of service {circuitBreakerServiceName} failed. Action url: {webEx?.Response?.ResponseUri}");
        }

        public bool IsExceptionIgnorable(Exception exception)
        {
            return false;
        }
    }


}