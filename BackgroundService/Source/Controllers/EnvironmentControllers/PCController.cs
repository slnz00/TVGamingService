using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;
using System.Windows.Forms;

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

            RestoreDisplaySettings();
            
            Services.OS.SoundDevice.SetDefaultSoundDevice(Config.Sound.DeviceName);

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
            BackupDisplaySettings();

            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
            Services.ThirdParty.Playnite.CloseDesktopPlaynite();
        }

        private void BackupDisplaySettings()
        {
            Services.OS.Display.BackupDisplaySettings();
        }

        private void RestoreDisplaySettings()
        {
            var result = Services.OS.Display.RestoreDisplaySettings();

            if (!result)
            {
                Services.OS.Window.ShowMessageBoxAsync(
                    MessageBoxIcon.Error,
                    "Failed to restore PC environment's display settings. Please reset your display settings manually."
                );
            }
        }
    }
}
