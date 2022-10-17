using System.Collections.Generic;
using TVGamingService.Source.Services;
using TVGamingService.Source.Services.Apps;
using TVGamingService.Source.Utils;

namespace TVGamingService.Source.Providers
{
    internal class ServiceProvider
    {
        private readonly LoggerProvider Logger;
        private readonly List<BaseService> services = new List<BaseService>();

        public class AppServices
        {
            public GameStoreService GameStore;
            public DS4WindowsService DS4Windows;
            public PlayniteService Playnite;
        }

        public readonly ConfigService Config;
        public readonly ConsoleService Console;
        public readonly DesktopService Desktop;
        public readonly HotkeyService Hotkey;
        public readonly LegacyDisplayService LegacyDisplay;
        public readonly SoundDeviceService SoundDevice;
        public readonly CursorService Cursor;

        public readonly AppServices Apps;

        // Initialization order is based on service registration order:
        public ServiceProvider()
        {
            Logger = new LoggerProvider(GetType().Name);

            Config = RegisterService(new ConfigService(this));
            Console = RegisterService(new ConsoleService(this));
            Desktop = RegisterService(new DesktopService(this));
            Hotkey = RegisterService(new HotkeyService(this));
            LegacyDisplay = RegisterService(new LegacyDisplayService(this));
            SoundDevice = RegisterService(new SoundDeviceService(this));
            Cursor = RegisterService(new CursorService(this));

            Apps = new AppServices
            {
                GameStore = RegisterService(new GameStoreService(this)),
                DS4Windows = RegisterService(new DS4WindowsService(this)),
                Playnite = RegisterService(new PlayniteService(this)),
            };
        }

        public void Initialize()
        {
            Logger.Debug("Initializing started");

            var time = TimerUtils.MeasurePerformance(() => services.ForEach(s => s.Initialize()));

            Logger.Info($"Successfully initialized, took: {time}ms");
        }

        private TService RegisterService<TService>(TService service) where TService : BaseService
        {
            services.Add(service);
            return service;
        }
    }
}
