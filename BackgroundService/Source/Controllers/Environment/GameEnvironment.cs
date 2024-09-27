using BackgroundService.Source.Controllers.Environment.Components;
using BackgroundService.Source.Providers;
using Core.Models.Configs;
using Core.Utils;
using System.Collections.Generic;
using System.Windows;

namespace BackgroundService.Source.Controllers.Environment
{
    internal class GameEnvironment : EnvironmentController
    {
        private GameEnvironmentConfig Config => Services.Config.GetConfig().GameEnvironment;

        private readonly List<uint> playniteListenerIds = new List<uint>();

        public GameEnvironment(MainController mainController, ServiceProvider services) :
            base(Environments.Game, mainController, services)
        { }

        protected override bool OnValidate()
        {
            if (!ValidatePlaynite())
            {
                return false;
            }

            return true;
        }

        protected override void OnSetup()
        {
            HideCursor();
            SetupDisplay();

            SetupAudio();
            SetupDesktop();

            CloseThirdPartyApps();
            OpenThirdPartyApps();

            LoadGameConfigs();
        }

        protected override void OnReset()
        {
            CloseThirdPartyApps();
            ForceCloseAppsOnGameDesktop();

            OpenThirdPartyApps();
        }

        protected override void OnTeardown()
        {
            CloseThirdPartyApps();

            SaveGameConfigs();
        }

        private bool ValidatePlaynite()
        {
            var playniteAvailable = Services.ThirdParty.Playnite.IsPlayniteAvailable();

            if (!playniteAvailable)
            {
                ShowErrorNotification(
                    "Playnite configuration is invalid. Please use the Configurator app to reconfigure it."
                );
            }

            return playniteAvailable;
        }

        private void LoadGameConfigs()
        {
            Services.GameConfig.LoadGameConfigsForEnvironment(EnvironmentType);
        }

        private void SaveGameConfigs()
        {
            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
        }

        private void ShowCursor()
        {
            Services.OS.Cursor.SetCursorVisibility(true);
        }

        private void HideCursor()
        {
            Services.OS.Cursor.SetCursorVisibility(false);
        }

        private void SetupDesktop()
        {
            Services.OS.Desktop.CreateAndSwitchToDesktop(InternalSettings.DesktopNameGameEnvironment);
            Services.OS.Desktop.ToggleIconsVisiblity(false);

            if (OSUtils.IsWindows11())
            {
                Services.OS.Desktop.ChangeWallpaperOnCurrentDesktop(Config.WallpaperPath);
            }
            else
            {
                Services.OS.Desktop.ChangeWallpaper(Config.WallpaperPath);
            }
        }

        private void SetupAudio()
        {
            Services.OS.Audio.StopMedia();

            Services.OS.Audio.SetDefaultAudioDevices(Config.Audio.InputDeviceName, Config.Audio.OutputDeviceName);
        }

        private void SetupDisplay()
        {
            var result = Services.OS.Display.SwitchToDisplay(Config.Display.DevicePath, Config.Display.DeviceName);

            if (!result)
            {
                ShowErrorNotification(
                    "Failed to switch displays. TV environment's display device is unavailable. Please use the Configurator app to reconfigure it."
                );
            }
        }

        private void OpenThirdPartyApps()
        {
            ClearPlayniteListeners();

            playniteListenerIds.Add
            (
                Services.ThirdParty.Playnite.OnPlayniteClosed(() =>
                {
                    MainController.ChangeEnvironmentTo(Environments.PC);
                })
            );
            playniteListenerIds.Add
            (
                Services.ThirdParty.Playnite.OnPlayniteOpened(() =>
                {
                    Services.ThirdParty.Playnite.FocusFullscreenPlaynite();
                })
            );
            playniteListenerIds.Add
            (
                Services.ThirdParty.Playnite.OnGameStopped((gameInfo) =>
                {
                    Services.ThirdParty.Playnite.FocusFullscreenPlaynite();
                })
            );

            Services.ThirdParty.DS4Windows.OpenDS4Windows();
            Services.ThirdParty.Playnite.OpenFullscreenPlaynite();
        }

        private void CloseThirdPartyApps()
        {
            ClearPlayniteListeners();

            Services.ThirdParty.DS4Windows.CloseDS4Windows();
            Services.ThirdParty.Playnite.CloseFullscreenPlaynite();
            Services.ThirdParty.GameStore.CloseAllGameStores();
        }

        private void ClearPlayniteListeners()
        {
            playniteListenerIds.ForEach(Services.ThirdParty.Playnite.RemoveEventListener);
            playniteListenerIds.Clear();
        }

        private void ForceCloseAppsOnGameDesktop()
        {
            var currentDesktopName = Services.OS.Desktop.GetCurrentDesktopName();
            var currentDesktopId = Services.OS.Desktop.GetCurrentDesktopId();

            if (currentDesktopName != InternalSettings.DesktopNameGameEnvironment)
            {
                return;
            }

            var windows = Services.OS.Desktop.GetWindowsOnDesktop(currentDesktopId);

            windows.ForEach(win => ProcessUtils.CloseProcess(win.ProcessID, true));
        }

        private void ShowErrorNotification(string message)
        {
            ShowCursor();

            Services.OS.Window.ShowMessageBoxAsync(MessageBoxImage.Error, message);
        }
    }
}
