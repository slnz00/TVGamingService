using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.WinAPI.DisplayAPI;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class DisplaySettingsSnapshot
    {
        public DisplaySettings Settings;
        public DisplayDevice[] Displays;

        public DisplaySettingsSnapshot() { }

        public DisplaySettingsSnapshot(DisplaySettings settings, DisplayDevice[] displays)
        {
            Settings = settings;
            Displays = displays;
        }

        public DisplayDevice GetDisplayForPath(DISPLAYCONFIG_PATH_INFO path)
        {
            var id = path.targetInfo.id;
            var display = Displays
                .Where(dp => dp.TargetInfo.id == path.targetInfo.id)
                .FirstOrDefault();

            if (display == null)
            {
                var sourceId = path.sourceInfo.id;
                var targetId = path.targetInfo.id;

                throw new ArgumentException($"Display does not exist for path: source #{sourceId} - target #{targetId}");
            }

            return display;
        }
    }
}
