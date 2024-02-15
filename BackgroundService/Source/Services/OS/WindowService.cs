using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;

using static Core.WinAPI.WindowAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class WindowService : Service
    {
        public WindowService(ServiceProvider services) : base(services) { }

        public List<WindowComponent> GetProcessWindows(int processId)
        {
            var windows = new List<WindowComponent>();

            EnumThreadDelegate addWindow = (handle, lParam) =>
            {
                windows.Add(new WindowComponent("Window", handle));

                return true;
            };

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

        public List<WindowComponent> GetChildComponent(WindowComponent component, string componentType)
        {
            List<WindowComponent> components = new List<WindowComponent>();
            if (component.Handle == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            IntPtr curentHandle = IntPtr.Zero;
            Func<IntPtr> NextComponentHandle = () => FindWindowEx(component.Handle, curentHandle, componentType, null);

            while ((curentHandle = NextComponentHandle()) != IntPtr.Zero)
            {
                components.Add(new WindowComponent(componentType, curentHandle));
            }

            return components;
        }

        public Task ShowMessageBoxAsync(MessageBoxIcon icon, string message)
        {
            return Task.Run(() =>
            {
                ShowMessageBoxSync(icon, message);
            });
        }

        public void ShowMessageBoxSync(MessageBoxIcon icon, string message)
        {
            MessageBox.Show(message, InternalSettings.WindowTitle, MessageBoxButtons.OK, icon);
        }
    }
}
