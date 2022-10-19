﻿using System.Collections.Generic;
using BackgroundService.Source.Services;
using BackgroundService.Source.Services.Configs;
using BackgroundService.Source.Services.System;
using BackgroundService.Source.Services.ThirdParty;
using Core.Utils;

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

        public class SystemServices
        {
            public ConsoleService Console;
            public DesktopService Desktop;
            public HotkeyService Hotkey;
            public LegacyDisplayService LegacyDisplay;
            public SoundDeviceService SoundDevice;
            public CursorService Cursor;
        }

        public readonly ThirdPartyServices ThirdParty;
        public readonly SystemServices System;

        // Common services should be defined without grouping:
        public readonly ConfigService Config;

        // Initialization order is based on service registration order:
        public ServiceProvider()
        {
            Logger = new LoggerProvider(GetType().Name);

            Config = RegisterService(new ConfigService(this));

            System = new SystemServices
            {
                Console = RegisterService(new ConsoleService(this)),
                Desktop = RegisterService(new DesktopService(this)),
                Hotkey = RegisterService(new HotkeyService(this)),
                LegacyDisplay = RegisterService(new LegacyDisplayService(this)),
                SoundDevice = RegisterService(new SoundDeviceService(this)),
                Cursor = RegisterService(new CursorService(this)),
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

            Logger.Info($"Successfully initialized, took: {time}ms");
        }

        private TService RegisterService<TService>(TService service) where TService : Service
        {
            services.Add(service);
            return service;
        }
    }
}