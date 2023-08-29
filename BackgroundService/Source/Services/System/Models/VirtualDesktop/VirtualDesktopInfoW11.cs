﻿using BackgroundService.Source.Services.System.API.VirtualDesktop;
using System;

namespace BackgroundService.Source.Services.System.Models.VirtualDesktop
{
    internal class VirtualDesktopInfoW11
    {
        public int Index { get; private set; }
        public string Name => GetDesktopName();
        public Guid Id => GetDesktopGuid();
        public VirtualDesktopAPIW11.IVirtualDesktop DesktopInstance { get; private set; }

        public VirtualDesktopInfoW11(int index, VirtualDesktopAPIW11.IVirtualDesktop desktopInstance)
        {
            DesktopInstance = desktopInstance;
            Index = index;
        }

        public bool OwnsView(VirtualDesktopAPIW11.IApplicationView view)
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