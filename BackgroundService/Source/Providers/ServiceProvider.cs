using BackgroundService.Source.Services;
using BackgroundService.Source.Services.Configs;
using BackgroundService.Source.Services.Jobs;
using BackgroundService.Source.Services.OS;
using BackgroundService.Source.Services.State;
using BackgroundService.Source.Services.ThirdParty;
using BackgroundService.Source.Services.ThirdParty.Playnite;
using Core.Components.System;
using Core.Utils;
using System.Collections.Generic;

namespace BackgroundService.Source.Providers
{
    internal class ServiceProvider
    {
        private readonly LoggerProvider Logger;
        private readonly List<Service> services = new List<Service>();

        public class ThirdPartyServices
        {
            public GameStoreService GameStore;
            public DS4WindowsService DS4Windows;
            public PlayniteService Playnite;
        }

        public class OSServices
        {
            public ConsoleService Console;
            public DesktopService Desktop;
            public HotkeyService Hotkey;
            public DisplayService Display;
            public AudioService Audio;
            public CursorService Cursor;
            public WindowService Window;
        }

        public readonly ThirdPartyServices ThirdParty;
        public readonly OSServices OS;

        public readonly ConfigService Config;
        public readonly GameConfigService GameConfig;
        public readonly JobService Jobs;
        public readonly StateService State;

        public readonly MessageLoop MessageLoop;

        // Initialization order is based on service registration order:
        public ServiceProvider(MessageLoop messageLoop)
        {
            MessageLoop = messageLoop;
            Logger = new LoggerProvider(GetType().Name);

            Config = RegisterService(new ConfigService(this));
            GameConfig = RegisterService(new GameConfigService(this));
            Jobs = RegisterService(new JobService(this));
            State = RegisterService(new StateService(this));

            OS = new OSServices
            {
                Console = RegisterService(new ConsoleService(this)),
                Desktop = RegisterService(DesktopService.Create(this)),
                Hotkey = RegisterService(new HotkeyService(this)),
                Display = RegisterService(new DisplayService(this)),
                Audio = RegisterService(new AudioService(this)),
                Cursor = RegisterService(new CursorService(this)),
                Window = RegisterService(new WindowService(this)),
            };

            ThirdParty = new ThirdPartyServices
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

            Logger.Debug($"Successfully initialized, took: {time}ms");
        }

        private TService RegisterService<TService>(TService service) where TService : Service
        {
            services.Add(service);
            return service;
        }
    }
}
