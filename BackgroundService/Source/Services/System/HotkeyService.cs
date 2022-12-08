using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.System.Models;
using Core.Components.System;

namespace BackgroundService.Source.Services.System
{
    internal class HotkeyService : Service
    {
        private static readonly uint HOTKEY_ACTION_TIMEOUT = InternalSettings.TIMEOUT_HOTKEY_ACTION;

        private const int HOTKEY_ID_BASE = 5152;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        int hotkeyIdCounter = 0;

        Dictionary<int, HotkeyAction> hotkeys = new Dictionary<int, HotkeyAction>();

        public HotkeyService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            MessageLoop.RegisterMessageHandler(HandleMessage);
        }

        protected override void OnDispose()
        {
            foreach (var item in hotkeys)
            {
                var hotkey = item.Value;
                var id = item.Key;

                Logger.Debug($"Unregistering hotkey - {hotkey.Name}: {hotkey.KeyModifierName} + {hotkey.KeyName}");

                UnregisterHotKey(IntPtr.Zero, id);
            }
        }

        // Only supports single keymodifier based hotkeys
        //  - Valid: ALT + 1
        //  - Invalid: CTRL + ALT + 1
        public bool RegisterAction(string name, HotkeyDefinition def, Action action)
        {
            int hotkeyId = HOTKEY_ID_BASE + hotkeyIdCounter;

            Logger.Debug($"Registering hotkey: {name} - ID: {hotkeyId}");

            bool result = RegisterHotKey(IntPtr.Zero, hotkeyId, (int)def.KeyModifier, def.Key.GetHashCode());
            if (result)
            {
                var hotkey = new HotkeyAction(def, name, WrapActionWithErrorHandler(name, action), HOTKEY_ACTION_TIMEOUT);

                hotkeys.Add(hotkeyId, hotkey);
                hotkeyIdCounter++;

                Logger.Info($"Hotkey successfully registered: {name} -> {def.KeyModifierName} + {def.KeyName}");
            }
            else
            {
                Logger.Error($"Failed to register hotkey: {name} -> {def.KeyModifierName} + {def.KeyName}");
            }

            return result;
        }

        private Action WrapActionWithErrorHandler(string name, Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to execute hotkey action {name}, exception:{e}");
                }
            };
        }

        private void HandleMessage(MessageLoop.MSG msg)
        {
            bool isHotkeyMessage = msg.message == WM_HOTKEY;
            if (isHotkeyMessage)
            {
                int hotkeyId = (int)msg.wParam.ToUInt32();

                bool hotkeyRegistered = hotkeys.TryGetValue(hotkeyId, out HotkeyAction hotkey);
                if (hotkeyRegistered && hotkey.IsActive)
                {
                    Logger.Debug($"Hotkey triggered: {hotkey.Name} -> {hotkey.KeyModifierName} + {hotkey.KeyName}");

                    hotkey.TriggerAction();
                }
            }
        }
    }
}
