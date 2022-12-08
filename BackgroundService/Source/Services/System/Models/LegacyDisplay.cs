using static Core.Components.System.DisplayWinApi;

namespace BackgroundService.Source.Services.System.Models
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
            return $"LegacyDisplay(adapter: '{AdapterDevice.DeviceID} - {AdapterDevice.DeviceName}', monitor: '{MonitorDevice.DeviceID} - {MonitorDevice.DeviceName}'";
        }

        private string GetModelNumber()
        {
            string[] deviceIdParts = MonitorDevice.DeviceID.Split('\\');
            return deviceIdParts[1];
        }

        private string GetGUID()
        {
            string[] deviceIdParts = MonitorDevice.DeviceID.Split('\\');
            return deviceIdParts[2].Replace("{", "").Replace("}", "");
        }
    }
}
