using System;
using TVGamingService.Source.Providers;
using TVGamingService.Source.Utils;

namespace TVGamingService.Source.Services
{
    internal abstract class BaseService : IDisposable
    {
        private bool initialized = false;
        private ServiceProvider services;
        private LoggerProvider logger;

        protected ServiceProvider Services => services;
        protected LoggerProvider Logger => logger;

        public bool IsInitialized => initialized;

        public BaseService(ServiceProvider services)
        {
            this.services = services;
            logger = new LoggerProvider(GetType().Name);
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
            Logger.Debug($"Disposing");

            var time = TimerUtils.MeasurePerformance(() => OnDispose());

            Logger.Debug($"Disposed, took: {time}ms");
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnDispose() { }
    }
}
