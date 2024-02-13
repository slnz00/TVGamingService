using System;
using System.Collections.Generic;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Controllers.EnvironmentControllers;
using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using Core.Components.System;
using System.Reflection;
using BackgroundService.Source.Services.OS.Models;
using BackgroundService.Source.Services.State.Components;
using Core.Utils;
using BackgroundService.Source.Services.OS;
using System.Threading;
using Core;
using System.Windows.Forms;

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

        private readonly BackupController.BackupController BackupController;
        private readonly Dictionary<Environments, Func<EnvironmentController>> EnvironmentFactory;

        private EnvironmentController CurrentEnvironment;

        private object threadLock = new object();

        public MainController()
        {
            ProcessMutex = new Mutex(true, SharedSettings.ProcessGuids.BACKGROUND_SERVICE.ToString());
            MessageLoop = new MessageLoop();
            Logger = new LoggerProvider(GetType().Name);
            Services = new ServiceProvider(MessageLoop);

            BackupController = new BackupController.BackupController(Services);

            EnvironmentFactory = new Dictionary<Environments, Func<EnvironmentController>>
            {
                { Environments.PC, () => new PCController(this, Services) },
                { Environments.TV, () => new TVController(this, Services) }
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
                ProcessMutex.ReleaseMutex();
            }
        }

        private void AcquireProcessMutex()
        {
            var alreadyRunning = !ProcessMutex.WaitOne(TimeSpan.Zero, true);

            if (alreadyRunning)
            {
                Services.OS.Window.ShowMessageBoxSync(MessageBoxIcon.Error, "A background service instance is already running.");

                Environment.Exit(-1);
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
                var environmentName = EnumUtils.GetName(environment);

                LogControllerEvent($"Switching environment to: {environmentName}");

                bool factoryExists = EnvironmentFactory.TryGetValue(environment, out var createEnvironmentController);
                if (!factoryExists)
                {
                    throw new KeyNotFoundException($"Environment does not have a factory method: {environmentName}");
                }

                var newController = createEnvironmentController();
                var currentController = CurrentEnvironment;

                currentController?.Teardown();
                newController.Setup();

                CurrentEnvironment = newController;
                EnvironmentState = environment;
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
