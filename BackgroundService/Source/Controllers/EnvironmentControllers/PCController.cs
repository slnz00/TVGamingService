using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal class PCController : EnvironmentController
    {
        public PCController(MainController mainController, ServiceProvider services) :
            base(Environments.PC, services.Config.GetConfig().PC, services.Config.GetJobsConfig().PC, mainController, services) { }

        protected override void OnSetup()
        {
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            Services.System.Cursor.SetCursorVisibility(true);

            // Change display and sound device:
            Services.System.LegacyDisplay.SwitchToDisplay(Config.Display);
            Services.System.SoundDevice.SetDefaultSoundDevice(Config.SoundDevice.DeviceName);

            // Change windows desktop, show desktop icons:
            Services.System.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.System.Desktop.RemoveDesktop(tvDesktopName);
            Services.System.Desktop.ToggleIconsVisiblity(true);
        }

        protected override void OnReset()
        {
            var tvDesktopName = InternalSettings.DESKTOP_TV_NAME;

            Services.System.Cursor.SetCursorVisibility(true);

            Services.System.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.System.Desktop.RemoveDesktop(tvDesktopName);
            Services.System.Desktop.ToggleIconsVisiblity(true);
        }

        protected override void OnTeardown() { }
    }
}
