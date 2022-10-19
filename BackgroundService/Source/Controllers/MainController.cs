using System;
using System.Collections.Generic;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Controllers.EnvironmentControllers;
using Core.SystemComponents;
using BackgroundService.Source.Controllers.Models;

namespace BackgroundService.Source.Controllers
{
    internal class MainController
    {
        private readonly LoggerProvider Logger = new LoggerProvider("Controller");
        private readonly ServiceProvider Services = new ServiceProvider();

        private Dictionary<Environments, Func<EnvironmentController>> EnvironmentControllerFactory;
        private EnvironmentController EnvironmentController = null;

        private object threadLock = new object();

        public void Run()
        {
            DisplayStartupTitle();
            InitializeComponents();
            SetupEnvironmentControllerFactory();
            SetupHotkeys();

            MessageLoop.Run();
        }

        private void InitializeComponents()
        {
            Services.Initialize();
        }

        private void SetupHotkeys()
        {
            Services.System.Hotkey.RegisterAction("SwitchEnvironment", InternalSettings.HOTKEY_SWITCH_ENVIRONMENT, SwitchEnvironment);
            Services.System.Hotkey.RegisterAction("ResetEnvironment", InternalSettings.HOTKEY_RESET_ENVIRONMENT, ResetEnvironment);
            Services.System.Hotkey.RegisterAction("ToggleConsoleVisibility", InternalSettings.HOTKEY_TOGGLE_CONSOLE_VISIBILITY, ToggleConsoleVisibility);
            Services.System.Hotkey.RegisterAction("ToggleCursorVisibility", InternalSettings.HOTKEY_TOGGLE_CURSOR_VISIBILITY, ToggleCursorVisibility);
        }

        private void SetupEnvironmentControllerFactory()
        {
            EnvironmentControllerFactory = new Dictionary<Environments, Func<EnvironmentController>>();

            EnvironmentControllerFactory.Add(Environments.PC, () => new PCController(this, Services));
            EnvironmentControllerFactory.Add(Environments.TV, () => new TVController(this, Services));
        }

        public void SwitchEnvironment()
        {
            lock (threadLock)
            {
                // Default, startup environment is PC:
                var isStartupEnvironment = EnvironmentController == null;
                var isPcEnvironment = isStartupEnvironment || EnvironmentController.Environment == Environments.PC;
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

                bool controllerExist = EnvironmentControllerFactory.TryGetValue(environment, out var createEnvironmentController);
                if (!controllerExist)
                {
                    Logger.Error($"Failed to change environment, controller instance does not exist for environment: {GetEnvironmentName(environment)}");
                    return;
                }

                var newController = createEnvironmentController();
                var currentController = EnvironmentController;

                currentController?.Teardown();
                newController.Setup();

                EnvironmentController = newController;
            }
        }

        private void ResetEnvironment()
        {
            lock (threadLock)
            {
                LogControllerEvent($"Resetting environment: {EnvironmentController.EnvironmentName}");

                EnvironmentController.Reset();
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
