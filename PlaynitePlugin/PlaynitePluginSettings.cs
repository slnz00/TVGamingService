﻿using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace PlaynitePlugin
{
    public class PlaynitePluginSettings : ObservableObject { }

    public class PlaynitePluginSettingsViewModel : ObservableObject, ISettings
    {
        private readonly PlaynitePlugin plugin;
        private PlaynitePluginSettings editingClone;

        private PlaynitePluginSettings settings;
        public PlaynitePluginSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public PlaynitePluginSettingsViewModel(PlaynitePlugin plugin)
        {
            this.plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<PlaynitePluginSettings>();

            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new PlaynitePluginSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}