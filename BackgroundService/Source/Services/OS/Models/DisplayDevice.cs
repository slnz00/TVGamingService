using static Core.WinAPI.DisplayAPI;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class DisplayDevice
    {
        public DISPLAYCONFIG_PATH_TARGET_INFO TargetInfo;
        public DISPLAYCONFIG_TARGET_DEVICE_NAME NameInfo;
        public DISPLAYCONFIG_TARGET_PREFERRED_MODE PreferredMode;

        public string FullName
        {
            get
            {
                var baseName = NameInfo.monitorFriendlyDeviceName;
                var devicePath = NameInfo.monitorDevicePath;

                if (string.IsNullOrWhiteSpace(baseName) || string.IsNullOrWhiteSpace(devicePath))
                {
                    return null;
                };

                var devicePathParts = devicePath.Split('#');

                if (devicePathParts.Length < 2)
                {
                    return baseName;
                }

                var modelName = devicePathParts[1];

                return $"{baseName} ({modelName})";
            }
        }

        public DisplayDevice(
            DISPLAYCONFIG_PATH_TARGET_INFO targetInfo,
            DISPLAYCONFIG_TARGET_DEVICE_NAME nameInfo,
            DISPLAYCONFIG_TARGET_PREFERRED_MODE preferredMode
        )
        {
            TargetInfo = targetInfo;
            NameInfo = nameInfo;
            PreferredMode = preferredMode;
        }
    }
}
