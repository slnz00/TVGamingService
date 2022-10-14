using System;
using TVGamingService.Source.Models;
using TVGamingService.Source.Providers;
using TVGamingService.Source.SystemComponents;

using static TVGamingService.Source.Services.Apps.GameStoreService;
using static TVGamingService.Source.Services.Apps.PlayniteService;

namespace TVGamingService.Source
{
    static class Controller
    {
        static readonly LoggerProvider Logger = new LoggerProvider(typeof(Controller).Name);
        static readonly ServiceProvider Services = new ServiceProvider();
        static readonly StateProvider State = new StateProvider();

        static readonly PlayniteEvents playniteEvents = new PlayniteEvents
        {
            onPlayniteClosed = () => State.ChangeEnvironment(Environments.PC)
        };

        static void Main()
        {
            DisplayStartupTitle();
            InitializeComponents();
            RegisterStateChangeActions();
            SetupHotkeys();

            MessageLoop.Run();
        }

        static void InitializeComponents()
        {
            LogControllerEvent("Initializing components");

            Services.Initialize();
        }

        static void SetupHotkeys()
        {
            LogControllerEvent("Setting up hotkeys");

            Services.Hotkey.RegisterAction("SwitchEnvironments", InternalSettings.HOTKEY_SWITCH_ENVIRONMENTS, SwitchEnvironment);
            Services.Hotkey.RegisterAction("SwitchEnvironments", InternalSettings.HOTKEY_RESET_ENVIRONMENT, ResetEnvironment);
            Services.Hotkey.RegisterAction("ToggleConsoleVisibility", InternalSettings.HOTKEY_TOGGLE_CONSOLE_VISIBILITY, ToggleConsoleVisibility);
            // TODO: Implement environment reset + hotkey
        }

        static void RegisterStateChangeActions()
        {
            State.RegisterEnvironmentChangeAction(Environments.TV, Environments.PC, () => SwitchToPC());
            State.RegisterEnvironmentChangeAction(Environments.PC, Environments.TV, () => SwitchToTV());
        }

        static void SwitchEnvironment()
        {
            var currentEnv = State.Environment;

            switch (currentEnv)
            {
                case Environments.PC:
                    State.ChangeEnvironment(Environments.TV);
                    return;
                case Environments.TV:
                    State.ChangeEnvironment(Environments.PC);
                    return;
            }
        }

        static void ResetEnvironment()
        {
            var currentEnv = State.Environment;

            switch (currentEnv)
            {
                case Environments.PC:
                    ResetPC();
                    return;
                case Environments.TV:
                    ResetTV();
                    return;
            }
        }

        static void ToggleConsoleVisibility()
        {
            LogControllerEvent("Toggling console visibility");

            Services.Console.ToggleConsoleVisibility();
        }

        static void SwitchToPC()
        {
            LogControllerEvent("Switching to PC environment");

            var pcConfig = Services.Config.GetConfig().PC;
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            // Change display and sound device:
            Services.LegacyDisplay.SwitchToDisplay(pcConfig.Display);
            Services.SoundDevice.SetDefaultSoundDevice(pcConfig.SoundDevice.DeviceName);

            // Change windows desktop, show desktop icons:
            Services.Desktop.RemoveDesktop(tvDesktopName);
            Services.Desktop.ToggleIconsVisiblity();

            // Close third party apps:
            Services.Apps.Playnite.ClosePlaynite();
            Services.Apps.DS4Windows.CloseDS4Windows();
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.STEAM);
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.EPIC_GAMES);
        }

        static void ResetPC()
        {
            LogControllerEvent("Resetting PC environment");

            Services.Apps.Playnite.ClosePlaynite();
            Services.Apps.DS4Windows.CloseDS4Windows(true);
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.STEAM);
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.EPIC_GAMES);
        }

        static void SwitchToTV()
        {
            LogControllerEvent("Switching to TV environment");

            var tvConfig = Services.Config.GetConfig().TV;
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            // Change display and sound device:
            Services.LegacyDisplay.SwitchToDisplay(tvConfig.Display);
            Services.SoundDevice.SetDefaultSoundDevice(tvConfig.SoundDevice.DeviceName);

            // Change windows desktop, hide desktop icons:
            Services.Desktop.CreateAndSwitchToDesktop(tvDesktopName);
            Services.Desktop.ToggleIconsVisiblity();

            // Close third party apps to make sure they get a clean start:
            Services.Apps.Playnite.ClosePlaynite();
            Services.Apps.DS4Windows.CloseDS4Windows();
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.STEAM);
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.EPIC_GAMES);

            // Open Playnite and DS4Windows:
            Services.Apps.DS4Windows.OpenDS4Windows();
            Services.Apps.Playnite.OpenPlaynite(playniteEvents);
        }

        static void ResetTV()
        {
            LogControllerEvent("Resetting TV environment");

            Services.Apps.Playnite.ClosePlaynite();
            Services.Apps.DS4Windows.CloseDS4Windows(true);
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.STEAM);
            Services.Apps.GameStore.CloseGameStore(GameStoreTypes.EPIC_GAMES);

            Services.Apps.DS4Windows.OpenDS4Windows();
            Services.Apps.Playnite.OpenPlaynite(playniteEvents);
        }

        static void LogControllerEvent(string message)
        {
            Logger.LogEmptyLine();
            Logger.Info($"EVENT: {message}...");
            Logger.LogEmptyLine();
        }

        static void DisplayStartupTitle()
        {
            Console.WriteLine($"[TV Gaming Service]\n[Startup: {DateTime.Now}]");
        }
    }
}
