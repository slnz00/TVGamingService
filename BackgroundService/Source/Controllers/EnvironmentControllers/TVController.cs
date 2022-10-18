using BackgroundService.Source.Controllers.Models;
using BackgroundService.Source.Providers;
using static BackgroundService.Source.Services.ThirdParty.PlayniteService;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal class TVController : EnvironmentController
    {
        private PlayniteEvents playniteEvents;

        public TVController(MainController mainController, ServiceProvider services) :
            base(Environments.TV, services.Config.GetConfig().TV, mainController, services)
        {
            playniteEvents = new PlayniteEvents()
            {
                onPlayniteClosed = () => MainController.ChangeEnvironmentTo(Environments.PC)
            };
        }

        protected override void OnSetup()
        {
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            Services.System.Cursor.SetCursorVisibility(false);

            // Change display and sound device:
            Services.System.LegacyDisplay.SwitchToDisplay(Config.Display);
            Services.System.SoundDevice.SetDefaultSoundDevice(Config.SoundDevice.DeviceName);

            // Change windows desktop, hide desktop icons:
            Services.System.Desktop.CreateAndSwitchToDesktop(tvDesktopName);
            Services.System.Desktop.ToggleIconsVisiblity();

            // Close third party apps to make sure they get a clean start:
            Services.ThirdParty.Playnite.ClosePlaynite();
            Services.ThirdParty.DS4Windows.CloseDS4Windows();
            Services.ThirdParty.GameStore.CloseAllGameStores();

            // Open DS4Windows and Playnite:
            Services.ThirdParty.DS4Windows.OpenDS4Windows();
            Services.ThirdParty.Playnite.OpenPlaynite(playniteEvents);

        }

        protected override void OnReset()
        {
            Services.System.Cursor.SetCursorVisibility(false);

            Services.ThirdParty.Playnite.ClosePlaynite();
            Services.ThirdParty.DS4Windows.CloseDS4Windows(true);
            Services.ThirdParty.GameStore.CloseAllGameStores();

            Services.ThirdParty.DS4Windows.OpenDS4Windows();
            Services.ThirdParty.Playnite.OpenPlaynite(playniteEvents);
        }

        protected override void OnTeardown() { }
    }
}
