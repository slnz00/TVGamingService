using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using Core.Components;
using Core.Components.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BackgroundService.Source.Services.OS
{
    internal class HotkeyService : Service
    {
        private static readonly uint HOTKEY_ACTION_TIMEOUT = InternalSettings.TimeoutHotkeyAction;

        private const int WM_HOTKEY = 0x0312;

        private readonly MessageLoop MessageLoop;

        private ManagedTask CurrentAction;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private int hotkeyIdCounter = 0;

        private readonly Dictionary<int, HotkeyAction> hotkeys = new Dictionary<int, HotkeyAction>();

        public HotkeyService(ServiceProvider services) : base(services)
        {
            MessageLoop = services.MessageLoop;
        }

        protected override void OnInitialize()
        {
            MessageLoop.RegisterMessageHandler(HandleMessage);
        }

        protected override void OnDispose()
        {
            UnregisterAllActions();
        }

        public void UnregisterAllActions()
        {
            MessageLoop.ExecuteOnMainThread(() =>
            {
                foreach (var item in hotkeys)
                {
                    var hotkey = item.Value;
                    var id = item.Key;

                    var result = UnregisterHotKey(IntPtr.Zero, id);

                    if (result)
                    {
                        Logger.Debug($"Hotkey unregistered: {hotkey.Name} -> {hotkey.KeyModifierName} + {hotkey.KeyName}");
                    }
                    else
                    {
                        Logger.Error($"Failed to unregister hotkey: {hotkey.Name} -> {hotkey.KeyModifierName} + {hotkey.KeyName} - {GetLastErrorMessage()}");
                    }
                }

                hotkeys.Clear();
            });
        }

        // Only supports single keymodifier based hotkeys
        //  - Valid: ALT + 1
        //  - Invalid: CTRL + ALT + 1
        public void RegisterAction(string name, HotkeyDefinition def, Action action)
        {
            MessageLoop.ExecuteOnMainThread(() =>
            {
                int hotkeyId = hotkeyIdCounter;

                Logger.Debug($"Registering hotkey: {name} - ID: {hotkeyId}");

                bool result = RegisterHotKey(IntPtr.Zero, hotkeyId, (int)def.KeyModifier, def.Key.GetHashCode());
                if (result)
                {
                    var hotkey = new HotkeyAction(def, name, WrapAction(def, name, action), HOTKEY_ACTION_TIMEOUT);

                    hotkeys.Add(hotkeyId, hotkey);
                    hotkeyIdCounter++;

                    Logger.Info($"Hotkey registered: {name} -> {def.KeyModifierName} + {def.KeyName}");
                }
                else
                {
                    Logger.Error($"Failed to register hotkey: {name} -> {def.KeyModifierName} + {def.KeyName} - {GetLastErrorMessage()}");
                }
            });
        }

        private Action WrapAction(HotkeyDefinition def, string name, Action action)
        {
            return () =>
            {
                try
                {
                    Logger.Info($"Hotkey pressed: {name} -> {def.KeyModifierName} + {def.KeyName}");

                    action();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to execute hotkey action {name}, exception: {ex}");
                }
            };
        }

        private void HandleMessage(MessageLoop.MSG msg)
        {
            bool isHotkeyMessage = msg.message == WM_HOTKEY;
            if (isHotkeyMessage)
            {
                if (CurrentAction != null && CurrentAction.IsAlive)
                {
                    return;
                }

                int hotkeyId = (int)msg.wParam.ToUInt32();

                bool hotkeyRegistered = hotkeys.TryGetValue(hotkeyId, out HotkeyAction hotkey);
                if (hotkeyRegistered && hotkey.IsActive)
                {
                    Logger.Debug($"Hotkey triggered: {hotkey.Name} -> {hotkey.KeyModifierName} + {hotkey.KeyName}");

                    CurrentAction = ManagedTask.Run(
                        (ctx) => hotkey.TriggerAction(),
                        TaskCreationOptions.LongRunning
                    );
                }
            }
        }

        private string GetLastErrorMessage()
        {
            var exception = new Win32Exception(Marshal.GetLastWin32Error());

            return exception.Message;
        }
    }
}
