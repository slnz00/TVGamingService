﻿using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
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
            Services.System.Cursor.SetCursorVisibility(true);

            // Change display and sound device:
            Services.System.LegacyDisplay.SwitchToDisplay_Old(Config.Display);
            Services.System.SoundDevice.SetDefaultSoundDevice(Config.Sound.DeviceName);

            // Change windows desktop, show desktop icons:
            Services.System.Desktop.RemoveDesktop(InternalSettings.DESKTOP_TV_NAME);
            Services.System.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.System.Desktop.ToggleIconsVisiblity(true);

            Services.GameConfig.LoadGameConfigsForEnvironment(EnvironmentType);
        }

        protected override void OnReset()
        {
            Services.System.Cursor.SetCursorVisibility(true);

            Services.System.Desktop.ChangeWallpaper(Config.WallpaperPath);
            Services.System.Desktop.RemoveDesktop(InternalSettings.DESKTOP_TV_NAME);
            Services.System.Desktop.ToggleIconsVisiblity(true);
        }

        protected override void OnTeardown()
        {
            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
            Services.ThirdParty.Playnite.CloseDesktopPlaynite();
        }
    }
}
