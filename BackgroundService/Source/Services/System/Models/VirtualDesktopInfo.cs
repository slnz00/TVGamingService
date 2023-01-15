using BackgroundService.Source.Services.System.API;
using System;

namespace BackgroundService.Source.Services.System.Models
{
    internal class VirtualDesktopInfo
    {
        public int Index { get; private set; }
        public string Name => GetDesktopName();
        public Guid Id => GetDesktopGuid();
        public VirtualDesktopAPI.IVirtualDesktop DesktopInstance { get; private set; }

        public VirtualDesktopInfo(int index, VirtualDesktopAPI.IVirtualDesktop desktopInstance)
        {
            DesktopInstance = desktopInstance;
            Index = index;
        }

        public bool OwnsView(VirtualDesktopAPI.IApplicationView view)
        {
            view.GetVirtualDesktopId(out var ownerDesktopId);

            return ownerDesktopId == Id;
        }

        public string GetDesktopName()
        {
            try
            {
                string registryKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops\Desktops\{" + Id.ToString() + "}";

                object registryValue = Microsoft.Win32.Registry.GetValue(registryKey, "Name", null);
                return registryValue != null ? (string)registryValue : "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get desktop name for [GUID: {Id}]: {ex}");
                return "";
            }
        }

        private Guid GetDesktopGuid()
        {
            return DesktopInstance.GetId();
        }
    }
}
