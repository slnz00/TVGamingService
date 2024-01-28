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
            Services.OS.Cursor.SetCursorVisibility(false);

            SwitchToTVDisplay();

            Services.OS.SoundDevice.SetDefaultSoundDevice(Config.Sound.DeviceName);

            Services.OS.Desktop.CreateAndSwitchToDesktop(InternalSettings.DESKTOP_TV_NAME);
            Services.OS.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.OS.Desktop.ToggleIconsVisiblity(false);

            CloseThirdPartyApps();
            OpenThirdPartyApps();

            Services.GameConfig.LoadGameConfigsForEnvironment(EnvironmentType);
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

            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
        }

        private void SwitchToTVDisplay()
        {
            var result = Services.OS.Display.SwitchToDisplay(Config.Display.DevicePath, Config.Display.DeviceName);

            if (!result)
            {
                Services.OS.Window.ShowMessageBox(
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
