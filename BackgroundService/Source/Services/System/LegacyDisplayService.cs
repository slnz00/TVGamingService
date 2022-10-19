using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using BackgroundService.Source.Services.System.Models;
using static Core.SystemComponents.DisplayWinAPI;

namespace BackgroundService.Source.Services.System
{
    class LegacyDisplayService : Service
    {
        public LegacyDisplayService(ServiceProvider services) : base(services) { }

        public void SaveDisplaySettings()
        {
            ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);
        }

        public void SwitchToDisplay(DisplayConfig displayCfg)
        {
            var displays = GetDisplays();
            DisableDisplay(displays[0]);

            displays = Services.System.LegacyDisplay.GetDisplays();
            SetDisplayAsPrimary(displays[1], displayCfg.RefreshRate, displayCfg.Resolution);

            SaveDisplaySettings();
        }

        public List<LegacyDisplay> GetDisplays()
        {
            var displays = new List<LegacyDisplay>();

            var adapterDevice = new DISPLAY_DEVICE();
            adapterDevice.cb = Marshal.SizeOf(adapterDevice);

            var monitorDevice = new DISPLAY_DEVICE();
            monitorDevice.cb = Marshal.SizeOf(adapterDevice);

            for (uint adapterId = 0; EnumDisplayDevices(null, adapterId, ref adapterDevice, 0); adapterId++)
            {
                EnumDisplayDevices(adapterDevice.DeviceName, 0, ref monitorDevice, 0);

                displays.Add(new LegacyDisplay(adapterDevice, monitorDevice));

                monitorDevice = new DISPLAY_DEVICE();
                monitorDevice.cb = Marshal.SizeOf(adapterDevice);
                adapterDevice = new DISPLAY_DEVICE();
                adapterDevice.cb = Marshal.SizeOf(adapterDevice);
            }

            Logger.Debug($"Legacy displays queried, count: {displays.Count}");

            return displays;
        }

        public void SetDisplayAsPrimary(LegacyDisplay display, int refreshRate, DisplayResolutionConfig resolution)
        {
            Logger.Debug($"Setting display as primary: {display}");
            Logger.Debug($"Setting display parameters: refreshRate({refreshRate}), width({resolution.Width}), height({resolution.Height})");

            var deviceMode = new DEVMODE();
            EnumDisplaySettings(display.AdapterDevice.DeviceName, -1, ref deviceMode);

            deviceMode.dmDisplayFrequency = refreshRate;
            deviceMode.dmPelsWidth = resolution.Width;
            deviceMode.dmPelsHeight = resolution.Height;
            deviceMode.dmFields = DEVMODEFlags.PelsHeight | DEVMODEFlags.PelsWidth | DEVMODEFlags.DisplayFrequency;

            ChangeDisplaySettingsEx(
               display.AdapterDevice.DeviceName,
               ref deviceMode,
               (IntPtr)null,
               ChangeDisplaySettingsFlags.CDS_SET_PRIMARY | ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET,
               IntPtr.Zero
           );

            Logger.Info($"Display set as primary: {display}");
        }

        public void DisableDisplay(LegacyDisplay display)
        {
            Logger.Debug($"Disabling display: {display}");

            var deviceMode = new DEVMODE();
            EnumDisplaySettings(display.AdapterDevice.DeviceName, -1, ref deviceMode);

            deviceMode.dmPosition.x = 0;
            deviceMode.dmPosition.y = 0;
            deviceMode.dmPelsWidth = 0;
            deviceMode.dmPelsHeight = 0;
            deviceMode.dmFields = DEVMODEFlags.PelsHeight | DEVMODEFlags.PelsWidth | DEVMODEFlags.Position;

            ChangeDisplaySettingsEx(
                display.AdapterDevice.DeviceName,
                ref deviceMode,
                (IntPtr)null,
                ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET,
                IntPtr.Zero
            );
        }
    }
}
