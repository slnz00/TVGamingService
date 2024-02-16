using static Core.WinAPI.LegacyDisplayAPI;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class LegacyDisplay
    {
        public string ModelNumber;
        public string GUID;
        public DISPLAY_DEVICE AdapterDevice;
        public DISPLAY_DEVICE MonitorDevice;

        public LegacyDisplay(DISPLAY_DEVICE adapterDevice, DISPLAY_DEVICE monitorDevice)
        {
            AdapterDevice = adapterDevice;
            MonitorDevice = monitorDevice;
            ModelNumber = GetModelNumber();
            GUID = GetGUID();
        }

        public override string ToString()
        {
            return $"{AdapterDevice.DeviceName}\\{MonitorDevice.DeviceID}'";
        }

        private string GetModelNumber()
        {
            string[] deviceIdParts = MonitorDevice.DeviceID.Split('\\');

            if (deviceIdParts.Length >= 2)
            {
                return deviceIdParts[1];
            }

            return MonitorDevice.DeviceID;
        }

        private string GetGUID()
        {
            string[] deviceIdParts = MonitorDevice.DeviceID.Split('\\');

            if (deviceIdParts.Length >= 3)
            {
                return deviceIdParts[2].Replace("{", "").Replace("}", "");
            }

            return "";
        }
    }
}
