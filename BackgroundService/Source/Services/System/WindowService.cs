using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using BackgroundService.Source.Providers;

namespace BackgroundService.Source.Services.System
{
    internal class WindowService : Service
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        public class WindowComponent
        {
            public string Type { get; set; }
            public IntPtr Handle { get; set; }
            public string Name { get; set; }
            public bool IsValid => Handle != IntPtr.Zero;

            public WindowComponent(string type, IntPtr handle)
            {
                Type = type;
                Handle = handle;
                Name = GetName();
            }

            private string GetName()
            {
                if (!IsValid)
                {
                    return "";
                }

                var length = GetWindowTextLength(Handle) + 1;
                var nameBuffer = new StringBuilder(length);
                GetWindowText(Handle, nameBuffer, nameBuffer.Capacity);

                var name = nameBuffer.ToString();

                return name.Replace("&", " ").Trim();
            }
        }


        public WindowService(ServiceProvider services) : base(services) { }

        WindowComponent FindWindowByName(string name)
        {
            var handle = FindWindow(null, name);
            return new WindowComponent("Window", handle);
        }

        List<WindowComponent> GetChildComponents(WindowComponent window, string componentType)
        {
            List<WindowComponent> components = new List<WindowComponent>();
            if (window.Handle == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr curentHandle = IntPtr.Zero;
            Func<IntPtr> NextComponentHandle = () => FindWindowEx(window.Handle, curentHandle, componentType, null);

            while ((curentHandle = NextComponentHandle()) != IntPtr.Zero)
            {
                components.Add(new WindowComponent(componentType, curentHandle));
            }

            return components;
        }
    }
}
