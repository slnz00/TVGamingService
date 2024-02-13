﻿using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;
using Core.Utils;
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
            RestoreSettings();
            ResetDesktop();
            ResetCursor();
        }

        protected override void OnReset()
        {
            ResetDesktop();
            ResetCursor();
        }

        protected override void OnTeardown()
        {
            BackupSettings();
            CloseDesktopPlaynite();
        }

        private void ResetDesktop()
        {
            Services.OS.Desktop.RemoveDesktop(InternalSettings.DESKTOP_TV_NAME);
            Services.OS.Desktop.ToggleIconsVisiblity(true);
        }

        private void ResetCursor()
        {
            Services.OS.Cursor.SetCursorVisibility(true);
        }

        private void CloseDesktopPlaynite()
        {
            Services.ThirdParty.Playnite.CloseDesktopPlaynite();
        }

        private void BackupSettings()
        {
            if (!OSUtils.IsWindows11())
            {
                Services.OS.Desktop.BackupWallpaperSettings();
            }

            Services.OS.Display.BackupDisplaySettings();
            Services.OS.Audio.BackupAudioSettings();
            Services.GameConfig.SaveGameConfigsForEnvironment(EnvironmentType);
        }

        private void RestoreSettings()
        {
            bool result;

            if (!OSUtils.IsWindows11())
            {
                Services.OS.Desktop.RestoreWallpaperSettings();
            }

            result = Services.OS.Display.RestoreDisplaySettings();
            if (!result)
            {
                Services.OS.Window.ShowMessageBoxAsync(
                    MessageBoxIcon.Error,
                    "Failed to restore PC environment's display settings. Please set your display settings manually."
                );
            }

            result = Services.OS.Audio.RestoreAudioSettings();
            if (!result)
            {
                Services.OS.Window.ShowMessageBoxAsync(
                    MessageBoxIcon.Error,
                    "Failed to restore PC environment's audio settings. Please set your audio settings manually."
                );
            }

            Services.GameConfig.LoadGameConfigsForEnvironment(EnvironmentType);
        }
    }
}
