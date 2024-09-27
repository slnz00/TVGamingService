using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using static Core.WinAPI.WindowAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class WindowService : Service
    {
        public WindowService(ServiceProvider services) : base(services) { }

        public List<WindowComponent> GetProcessWindows(int processId)
        {
            var windows = new List<WindowComponent>();

            bool addWindow(IntPtr handle, IntPtr lParam)
            {
                windows.Add(new WindowComponent("Window", handle));

                return true;
            }

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                EnumThreadWindows(thread.Id, addWindow, IntPtr.Zero);
            }

            return windows;
        }

        public WindowComponent FindWindowByName(string name)
        {
            var handle = FindWindow(null, name);
            return new WindowComponent("Window", handle);
        }

        public List<WindowComponent> GetChildComponents(WindowComponent component, string componentType)
        {
            List<WindowComponent> components = new List<WindowComponent>();
            if (component.Handle == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            IntPtr curentHandle = IntPtr.Zero;
            IntPtr NextComponentHandle() => FindWindowEx(component.Handle, curentHandle, componentType, null);

            while ((curentHandle = NextComponentHandle()) != IntPtr.Zero)
            {
                components.Add(new WindowComponent(componentType, curentHandle));
            }

            return components;
        }

        public Task ShowMessageBoxAsync(MessageBoxImage icon, string message)
        {
            return Task.Run(() =>
            {
                ShowMessageBoxSync(icon, message);
            });
        }

        public void ShowMessageBoxSync(MessageBoxImage icon, string message)
        {
            MessageBox.Show(message, InternalSettings.WindowTitle, MessageBoxButton.OK, icon);
        }
    }
}
