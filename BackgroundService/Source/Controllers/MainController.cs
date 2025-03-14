﻿using BackgroundService.Source.Controllers.Environment;
using BackgroundService.Source.Controllers.Environment.Components;
using BackgroundService.Source.Controllers.Backup;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS;
using BackgroundService.Source.Services.OS.Models;
using BackgroundService.Source.Services.State.Components;
using Core;
using Core.Components.System;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace BackgroundService.Source.Controllers
{
    internal class MainController
    {
        private Environments EnvironmentState
        {
            get
            {
                var defaultName = EnumUtils.GetName(Environments.PC);
                var name = Services.State.Get<string>(States.CurrentEnvironment) ?? defaultName;

                if (!EnumUtils.IsValidName<Environments>(name))
                {
                    name = defaultName;

                    Logger.Error($"Current environment state is invalid reverting to default value: {name}");

                    Services.State.Set(States.CurrentEnvironment, name);
                }

                return EnumUtils.GetValue<Environments>(name);
            }

            set
            {
                var name = EnumUtils.GetName(value);

                Services.State.Set(States.CurrentEnvironment, name);
            }
        }

        private readonly Mutex ProcessMutex;
        private readonly MessageLoop MessageLoop;
        private readonly ServiceProvider Services;
        private readonly LoggerProvider Logger;

        private readonly BackupController BackupController;
        private readonly Dictionary<Environments, Func<EnvironmentController>> EnvironmentFactory;

        private EnvironmentController CurrentEnvironment;

        private readonly object threadLock = new object();

        public MainController()
        {
            ProcessMutex = new Mutex(true, SharedSettings.ProcessGuids.BackgroundService.ToString());
            MessageLoop = new MessageLoop();
            Logger = new LoggerProvider(GetType().Name);
            Services = new ServiceProvider(MessageLoop);

            BackupController = new BackupController(Services);

            EnvironmentFactory = new Dictionary<Environments, Func<EnvironmentController>>
            {
                { Environments.PC, () => new PCEnvironment(this, Services) },
                { Environments.Game, () => new GameEnvironment(this, Services) }
            };
        }

        public void Run()
        {
            Setup();

            BackupController.Start();
            MessageLoop.Run();
        }

        public void Stop()
        {
            BackupController.Stop();
            MessageLoop.Stop();

            Teardown();
        }

        private void Setup()
        {
            lock (threadLock)
            {
                AcquireProcessMutex();
                DisplayStartupTitle();
                InitializeComponents();
                InitializeEnvironment();
                SetupHotkeys();
            }
        }

        private void Teardown()
        {
            lock (threadLock)
            {
                CursorService.EnsureCursorIsVisible();
            }
        }

        private void AcquireProcessMutex()
        {
            var alreadyRunning = !ProcessMutex.WaitOne(TimeSpan.Zero, true);

            if (alreadyRunning)
            {
                Services.OS.Window.ShowMessageBoxSync(MessageBoxImage.Error, "A background service instance is already running.");

                System.Environment.Exit(-1);
            }
        }

        private void InitializeComponents()
        {
            Services.Initialize();
            BackupController.Initialize();

            ConsoleWindowHandler.OnExit = Stop;
            ConsoleWindowHandler.Initialize();
        }

        private void InitializeEnvironment()
        {
            CurrentEnvironment = EnvironmentFactory[EnvironmentState]();

            Logger.Info($"Startup environment: {CurrentEnvironment.EnvironmentName}");

            if (CurrentEnvironment.EnvironmentType != Environments.PC)
            {
                ChangeEnvironmentTo(Environments.PC);
            }

            CurrentEnvironment.EnsureSetupJobsAreCreated();
        }

        private void SetupHotkeys()
        {
            UpdateHotkeys();

            Services.Config.ConfigWatcher.OnChanged(() =>
            {
                UpdateHotkeys();
            });
        }

        private void UpdateHotkeys()
        {
            lock (threadLock)
            {
                Services.OS.Hotkey.UnregisterAllActions();

                var Hotkeys = Services.Config.GetConfig().Hotkeys;

                Services.OS.Hotkey.RegisterAction("SwitchEnvironment", new HotkeyDefinition(Hotkeys.SwitchEnvironment), SwitchEnvironment);
                Services.OS.Hotkey.RegisterAction("ResetEnvironment", new HotkeyDefinition(Hotkeys.ResetEnvironment), ResetEnvironment);
                Services.OS.Hotkey.RegisterAction("ToggleConsoleVisibility", new HotkeyDefinition(Hotkeys.ToggleConsoleVisibility), ToggleConsoleVisibility);
                Services.OS.Hotkey.RegisterAction("ToggleCursorVisibility", new HotkeyDefinition(Hotkeys.ToggleCursorVisibility), ToggleCursorVisibility);
            }
        }

        public void SwitchEnvironment()
        {
            lock (threadLock)
            {
                var isPC = CurrentEnvironment.EnvironmentType == Environments.PC;
                if (isPC)
                {
                    ChangeEnvironmentTo(Environments.Game);
                }
                else
                {
                    ChangeEnvironmentTo(Environments.PC);
                }
            }
        }

        public void ChangeEnvironmentTo(Environments environment)
        {
            lock (threadLock) { 
                var currentEnvironmentName = CurrentEnvironment.EnvironmentName;
                var environmentName = EnumUtils.GetName(environment);

                LogControllerEvent($"Switching environment to: {environmentName}");

                bool factoryExists = EnvironmentFactory.TryGetValue(environment, out var createEnvironment);
                if (!factoryExists)
                {
                    throw new KeyNotFoundException($"Environment does not have a factory method: {environmentName}");
                }

                var newEnvironment = createEnvironment();

                if (!newEnvironment.Validate())
                {
                    Logger.Error($"Failed to change environment: Validation failed");

                    return;
                }

                try {
                    CurrentEnvironment?.Teardown();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to teardown current environment {currentEnvironmentName}: {ex}");
                    return;
                }

                try
                {
                    newEnvironment.Setup();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to setup environment {environmentName}: {ex}");
                    return;
                }

                CurrentEnvironment = newEnvironment;
                EnvironmentState = newEnvironment.EnvironmentType;
            }
        }

        private void ResetEnvironment()
        {
            lock (threadLock)
            {
                LogControllerEvent($"Resetting environment: {CurrentEnvironment.EnvironmentName}");

                CurrentEnvironment.Reset();
            }
        }

        private void ToggleConsoleVisibility()
        {
            lock (threadLock)
            {
                LogControllerEvent("Toggling console visibility");

                Services.OS.Console.ToggleConsoleVisibility();
            }
        }

        private void ToggleCursorVisibility()
        {
            lock (threadLock)
            {
                LogControllerEvent("Toggling cursor visibility");

                var currentVisibility = Services.OS.Cursor.GetCursorVisibility();
                Services.OS.Cursor.SetCursorVisibility(!currentVisibility);
            }
        }

        private void LogControllerEvent(string message)
        {
            Logger.LogEmptyLine();
            Logger.Info($"EVENT: {message}...");
            Logger.LogEmptyLine();
        }

        private void DisplayStartupTitle()
        {
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            var versionString = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Logger.Plain($"[TV Gaming Service] [Version: {versionString}] [Startup: {now}]");
            Logger.LogEmptyLine();
        }
    }
}
