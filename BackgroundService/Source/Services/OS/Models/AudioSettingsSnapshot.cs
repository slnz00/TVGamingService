namespace BackgroundService.Source.Services.OS.Models
{
    internal class AudioSettingsSnapshot
    {
        internal class DeviceIDs
        {
            public string Multimedia;
            public string Communication;
        }

        public DeviceIDs DefaultInputIDs = new DeviceIDs();
        public DeviceIDs DefaultOutputIDs = new DeviceIDs();
    }
}
