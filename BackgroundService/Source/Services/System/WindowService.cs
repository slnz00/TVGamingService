using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.System.Models;

namespace BackgroundService.Source.Services.System
{
    internal class WindowService : Service
    {
        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);


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
    }
}
