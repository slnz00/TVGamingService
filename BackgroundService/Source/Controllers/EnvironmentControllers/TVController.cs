using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;
using Core.Utils;
using System.Windows.Forms;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal class TVController : EnvironmentController
    {
        private uint? playniteClosedListenerId = null;

        public TVController(MainController mainController, ServiceProvider services) :
            base(Environments.TV, mainController, services)
        { }

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
            ForceCloseAppsOnTVDesktop();

            OpenThirdPartyApps();
        }

        protected override void OnTeardown()
        {
            CloseThirdPartyApps();

            SaveGameConfigs();
        }

        private void LoadGameConfigs()
        {
            Services.GameConfig.LoadGameConfigsForEnvironment(EnvironmentType);
        }

        private void SaveGameConfigs()
        {
            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
        }

        private void HideCursor()
        {
            Services.OS.Cursor.SetCursorVisibility(false);
        }

        private void SetupDesktop()
        {
            Services.OS.Desktop.CreateAndSwitchToDesktop(InternalSettings.DESKTOP_TV_NAME);
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
                Services.OS.Window.ShowMessageBoxAsync(
                    MessageBoxIcon.Error,
                    "Failed to switch displays. TV environment's display device is unavailable. Please reconfigure it using the Configurator app."
                );
            }
        }

        private void OpenThirdPartyApps()
        {
            if (playniteClosedListenerId != null)
            {
                Services.ThirdParty.Playnite.RemoveEventListener((uint)playniteClosedListenerId);
            }

            playniteClosedListenerId = Services.ThirdParty.Playnite.OnPlayniteClosed(() =>
            {
                MainController.ChangeEnvironmentTo(Environments.PC);
            });

            Services.ThirdParty.DS4Windows.OpenDS4Windows();
            Services.ThirdParty.Playnite.OpenFullscreenPlaynite();
        }

        private void CloseThirdPartyApps()
        {
            if (playniteClosedListenerId != null)
            {
                Services.ThirdParty.Playnite.RemoveEventListener((uint)playniteClosedListenerId);
            }

            Services.ThirdParty.DS4Windows.CloseDS4Windows();
            Services.ThirdParty.Playnite.CloseFullscreenPlaynite();
            Services.ThirdParty.GameStore.CloseAllGameStores();
        }

        private void ForceCloseAppsOnTVDesktop()
        {
            var windows = Services.OS.Desktop.GetWindowsOnDesktop(InternalSettings.DESKTOP_TV_NAME);

            windows.ForEach(win => ProcessUtils.CloseProcess(win.ProcessID, true));
        }
    }
}
