using System;
using BackgroundService.Source.Controllers.Models;
using BackgroundService.Source.Providers;
using static BackgroundService.Source.Services.ThirdParty.GameStoreService;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal class PCController : EnvironmentController
    {
        public PCController(MainController mainController, ServiceProvider services) :
            base(Environments.PC, services.Config.GetConfig().PC, mainController, services) { }

        protected override void OnSetup()
        {
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            Services.System.Cursor.SetCursorVisibility(true);

            // Change display and sound device:
            Services.System.LegacyDisplay.SwitchToDisplay(Config.Display);
            Services.System.SoundDevice.SetDefaultSoundDevice(Config.SoundDevice.DeviceName);

            // Change windows desktop, show desktop icons:
            Services.System.Desktop.RemoveDesktop(tvDesktopName);
            Services.System.Desktop.ToggleIconsVisiblity();

            // Close third party apps:
            Services.ThirdParty.Playnite.ClosePlaynite();
            Services.ThirdParty.DS4Windows.CloseDS4Windows();
            Services.ThirdParty.GameStore.CloseAllGameStores();
        }

        protected override void OnReset()
        {
            Services.System.Cursor.SetCursorVisibility(true);

            Services.ThirdParty.Playnite.ClosePlaynite();
            Services.ThirdParty.DS4Windows.CloseDS4Windows(true);
            Services.ThirdParty.GameStore.CloseAllGameStores();
        }

        protected override void OnTeardown() { }
    }
}
