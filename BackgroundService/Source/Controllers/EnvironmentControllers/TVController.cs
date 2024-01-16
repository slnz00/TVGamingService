using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;
using Core.Utils;

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

            // Change display and sound device:
            Services.OS.LegacyDisplay.SwitchToDisplay_Old(Config.Display);
            Services.OS.SoundDevice.SetDefaultSoundDevice(Config.Sound.DeviceName);

            // Change windows desktop, hide desktop icons:
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
