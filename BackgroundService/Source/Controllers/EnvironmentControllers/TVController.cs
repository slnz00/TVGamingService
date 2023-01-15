using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal class TVController : EnvironmentController
    {
        private uint? playniteClosedListenerId = null;

        public TVController(MainController mainController, ServiceProvider services) :
            base(Environments.TV, services.Config.GetConfig().TV, services.Config.GetJobsConfig().TV, mainController, services)
        { }

        protected override void OnSetup()
        {
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            Services.System.Cursor.SetCursorVisibility(false);

            // Change display and sound device:
            Services.System.LegacyDisplay.SwitchToDisplay(Config.Display);
            Services.System.SoundDevice.SetDefaultSoundDevice(Config.SoundDevice.DeviceName);

            // Change windows desktop, hide desktop icons:
            Services.System.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.System.Desktop.CreateAndSwitchToDesktop(tvDesktopName);
            Services.System.Desktop.ToggleIconsVisiblity(false);

            CloseThirdPartyApps();
            OpenThirdPartyApps();

            Services.GameConfig.LoadGameConfigsForEnvironment(Environment);
        }

        protected override void OnReset()
        {
            Services.System.Cursor.SetCursorVisibility(false);

            CloseThirdPartyApps();
            OpenThirdPartyApps();
        }

        protected override void OnTeardown()
        {
            CloseThirdPartyApps();

            Services.GameConfig.SaveGameConfigsForEnvironment(Environment);
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
            Services.ThirdParty.Playnite.OpenPlaynite();
        }

        private void CloseThirdPartyApps()
        {
            if (playniteClosedListenerId != null)
            {
                Services.ThirdParty.Playnite.RemoveEventListener((uint)playniteClosedListenerId);
            }

            Services.ThirdParty.DS4Windows.CloseDS4Windows();
            Services.ThirdParty.Playnite.ClosePlaynite();
            Services.ThirdParty.GameStore.CloseAllGameStores();
        }
    }
}
