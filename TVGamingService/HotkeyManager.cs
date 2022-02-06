using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TVGamingService
{
    internal class HotkeyManager : IDisposable
    {
        [Flags]
        public enum KeyModifiers : uint
        {
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        const int HOTKEY_ID_BASE = 1;
        const int WM_HOTKEY = 0x0312;

        int hotkeyIdCounter = 0;

        Dictionary<int, Action> hotkeyHandlers = new Dictionary<int, Action>();

        public HotkeyManager()
        {
            MessageLoop.RegisterMessageHandler(HandleMessage);
        }

        public void Dispose()
        {
            foreach (var item in hotkeyHandlers)
            {
                UnregisterHotKey(IntPtr.Zero, item.Key);
            }
        }


        public bool RegisterAction(KeyModifiers keyModifiers, Keys key, Action action) {
            int hotkeyId = HOTKEY_ID_BASE + hotkeyIdCounter;

            bool result = RegisterHotKey(IntPtr.Zero, 1, (int)keyModifiers, key.GetHashCode());
            if (result) {
                hotkeyHandlers.Add(hotkeyId, action);
                hotkeyIdCounter++;
            }

            return result;
        }
    
        private void HandleMessage(MessageLoop.MSG msg)
        {
            if (msg.message == WM_HOTKEY)
            {
                int hotkeyId = (int)msg.wParam.ToUInt32();
                bool actionRegistered = hotkeyHandlers.TryGetValue(hotkeyId, out Action handler);
                if (actionRegistered) {
                    handler();
                }
            }
        }


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
