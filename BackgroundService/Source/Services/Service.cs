using System;
using BackgroundService.Source.Providers;
using Core.Utils;

namespace BackgroundService.Source.Services
{
    internal abstract class Service : IDisposable
    {
        private bool initialized = false;

        protected readonly ServiceProvider Services;
        protected readonly LoggerProvider Logger;

        public bool IsInitialized => initialized;

        public Service(ServiceProvider services)
        {
            Services = services;
            Logger = new LoggerProvider(GetType().Name);
        }

        public void Initialize()
        {
            if (initialized)
            {
                throw new InvalidOperationException("Service already initialized");
            }

            var time = TimerUtils.MeasurePerformance(() => OnInitialize());

            Logger.Debug($"Initialized, took: {time}ms");

            initialized = true;
        }

        public void Dispose()
        {
            var time = TimerUtils.MeasurePerformance(() => OnDispose());

            Logger.Debug($"Disposed, took: {time}ms");
        }

        protected void RequireInitialization()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Uninitialized service, call 'Initialize' method first");
            }
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnDispose() { }
    }
}
