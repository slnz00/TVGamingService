﻿using System;
using System.Collections.Generic;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Controllers.EnvironmentControllers;
using Core.Components.System;
using BackgroundService.Source.Controllers.EnvironmentControllers.Models;

namespace BackgroundService.Source.Controllers
{
    internal class MainController
    {
        private class SubControllers
        {
            public Dictionary<Environments, Func<EnvironmentController>> EnvironmentFactory;
            public EnvironmentController Environment;
            public BackupController.BackupController Backup;
        }

        private readonly ServiceProvider Services;
        private readonly LoggerProvider Logger;
        private readonly SubControllers Controllers;

        private object threadLock = new object();

        public MainController()
        {
            Logger = new LoggerProvider(GetType().Name);
            Services = new ServiceProvider();

            Controllers = new SubControllers
            {
                EnvironmentFactory = new Dictionary<Environments, Func<EnvironmentController>>
                {
                    { Environments.PC, () => new PCController(this, Services) },
                    { Environments.TV, () => new TVController(this, Services) }
                },
                Backup = new BackupController.BackupController(Services)
            };
            Controllers.Environment = Controllers.EnvironmentFactory[Environments.PC]();
        }

        public void Run()
        {
            DisplayStartupTitle();
            InitializeComponents();
            SetupHotkeys();

            Services.System.LegacyDisplay.GetDisplays();

            MessageLoop.Run();
        }

        private void InitializeComponents()
        {
            Services.Initialize();
            Controllers.Backup.Initialize();
        }

        private void SetupHotkeys()
        {
            Services.System.Hotkey.RegisterAction("SwitchEnvironment", InternalSettings.HOTKEY_SWITCH_ENVIRONMENT, SwitchEnvironment);
            Services.System.Hotkey.RegisterAction("ResetEnvironment", InternalSettings.HOTKEY_RESET_ENVIRONMENT, ResetEnvironment);
            Services.System.Hotkey.RegisterAction("ResetDisplay", InternalSettings.HOTKEY_RESET_DISPLAY, ResetDisplay);
            Services.System.Hotkey.RegisterAction("ToggleConsoleVisibility", InternalSettings.HOTKEY_TOGGLE_CONSOLE_VISIBILITY, ToggleConsoleVisibility);
            Services.System.Hotkey.RegisterAction("ToggleCursorVisibility", InternalSettings.HOTKEY_TOGGLE_CURSOR_VISIBILITY, ToggleCursorVisibility);
        }

        public void SwitchEnvironment()
        {
            lock (threadLock)
            {
                // Default, startup environment is PC:
                var isStartupEnvironment = Controllers.Environment == null;
                var isPcEnvironment = isStartupEnvironment || Controllers.Environment.EnvironmentType == Environments.PC;
                if (isPcEnvironment)
                {
                    ChangeEnvironmentTo(Environments.TV);
                }
                else
                {
                    ChangeEnvironmentTo(Environments.PC);
                }
            }
        }

        public void ChangeEnvironmentTo(Environments environment)
        {
            lock (threadLock)
            {
                LogControllerEvent($"Switching environment to: {GetEnvironmentName(environment)}");

                bool controllerExists = Controllers.EnvironmentFactory.TryGetValue(environment, out var createEnvironmentController);
                if (!controllerExists)
                {
                    Logger.Error($"Failed to change environment, controller instance does not exist for environment: {GetEnvironmentName(environment)}");
                    return;
                }

                var newController = createEnvironmentController();
                var currentController = Controllers.Environment;

                currentController?.Teardown();
                newController.Setup();

                Controllers.Environment = newController;
            }
        }

        private void ResetEnvironment()
        {
            lock (threadLock)
            {
                LogControllerEvent($"Resetting environment: {Controllers.Environment.EnvironmentName}");

                Controllers.Environment.Reset();
            }
        }

        private void ToggleConsoleVisibility()
        {
            lock (threadLock)
            {
                LogControllerEvent("Toggling console visibility");

                Services.System.Console.ToggleConsoleVisibility();
            }
        }

        private void ToggleCursorVisibility()
        {
            lock (threadLock)
            {
                LogControllerEvent("Toggling cursor visibility");

                var currentVisibility = Services.System.Cursor.CursorVisibility;
                Services.System.Cursor.SetCursorVisibility(!currentVisibility);
            }
        }

        private void ResetDisplay()
        {
            lock (threadLock)
            {
                LogControllerEvent("Resetting display");

                var environment = Controllers.Environment.EnvironmentType;
                var config = Services.Config.GetConfig();
                var environmentConfig = environment == Environments.PC ? config.PC : config.TV;

                Services.System.LegacyDisplay.SwitchToDisplay_Old(environmentConfig.Display);
            }
        }

        private string GetEnvironmentName(Environments environment)
        {
            return Enum.GetName(typeof(Environments), environment);
        }

        private void LogControllerEvent(string message)
        {
            Logger.LogEmptyLine();
            Logger.Info($"EVENT: {message}...");
            Logger.LogEmptyLine();
        }

        private void DisplayStartupTitle()
        {
            Console.WriteLine($"[TV Gaming Service]\n[Startup: {DateTime.Now}]");
        }
    }
}
