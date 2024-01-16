using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal class PCController : EnvironmentController
    {
        public PCController(MainController mainController, ServiceProvider services) :
            base(Environments.PC, mainController, services)
        { }

        protected override void OnSetup()
        {
            Services.OS.Cursor.SetCursorVisibility(true);

            // Change display and sound device:
            Services.OS.LegacyDisplay.SwitchToDisplay_Old(Config.Display);
            Services.OS.SoundDevice.SetDefaultSoundDevice(Config.Sound.DeviceName);

            // Change windows desktop, show desktop icons:
            Services.OS.Desktop.RemoveDesktop(InternalSettings.DESKTOP_TV_NAME);
            Services.OS.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.OS.Desktop.ToggleIconsVisiblity(true);

            Services.GameConfig.LoadGameConfigsForEnvironment(EnvironmentType);
        }

        protected override void OnReset()
        {
            Services.OS.Cursor.SetCursorVisibility(true);

            Services.OS.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.OS.Desktop.RemoveDesktop(InternalSettings.DESKTOP_TV_NAME);
            Services.OS.Desktop.ToggleIconsVisiblity(true);
        }

        protected override void OnTeardown()
        {
            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
            Services.ThirdParty.Playnite.CloseDesktopPlaynite();
        }
    }
}
