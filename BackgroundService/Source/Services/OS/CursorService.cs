using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.State;
using BackgroundService.Source.Services.State.Components;
using Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BackgroundService.Source.Services.OS
{
    internal class CursorService : Service
    {
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);

        private const int SPI_SETCURSORS = 0x0057;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        private static readonly string CURSORS_REGISTRY_SUB_KEY = @"Control Panel\Cursors";

        private static readonly List<string> CURSOR_REGISTRY_NAMES = new List<string> {
            "AppStarting",
            "Arrow",
            "ContactVisualization",
            "Crosshair",
            "Hand",
            "Help",
            "IBeam",
            "No",
            "NWPen",
            "SizeAll",
            "SizeNESW",
            "SizeNS",
            "SizeNWSE",
            "SizeWE",
            "UpArrow",
            "Wait",
            "Pin",
            "Person"
        };

        private List<CursorRegistryValue> CursorRegistrySnapshot
        {
            get
            {
                return Services.State.Get<List<CursorRegistryValue>>(States.CursorRegistrySnapshot);
            }
            set
            {
                Services.State.Set(States.CursorRegistrySnapshot, value);
            }
        }

        public class CursorRegistryValue
        {
            public string Name { get; set; }
            public string Path { get; set; }

            public CursorRegistryValue(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }

        public CursorService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            SaveRegistryToState();
        }

        public static void EnsureCursorIsVisible()
        {
            List<CursorRegistryValue> GetRegistryValuesFromState()
            {
                try
                {
                    var state = new StateService(null, true);

                    state.Initialize();

                    return state.Get<List<CursorRegistryValue>>(States.CursorRegistrySnapshot);
                }
                catch (Exception ex)
                {
                    LoggerProvider.Global.Error($"EnsureCursorIsVisible: Failed to get cursor registry values from state: {ex}");

                    return null;
                }
            }

            var currentRegistry = GetCurrentRegistry();
            var stateRegistry = GetRegistryValuesFromState();
            var defaultRegistry = GetDefaultRegistry();

            if (IsHiddenCursorRegistry(currentRegistry))
            {
                SetCursorRegistry(stateRegistry ?? defaultRegistry);
                ApplyCursorSettings();
            }
        }

        public void SetCursorVisibility(bool visible)
        {
            Logger.Info($"Setting cursor visibility to: {visible}");

            var currentVisibility = GetCursorVisibility();
            if (currentVisibility == visible)
            {
                return;
            }

            if (!visible)
            {
                SaveRegistryToState();
            }

            var registry = visible ? CursorRegistrySnapshot : GetHiddenCursorRegistry();

            SetCursorRegistry(registry);
            ApplyCursorSettings();
        }

        public bool GetCursorVisibility()
        {
            var registry = GetCurrentRegistry();

            return !IsHiddenCursorRegistry(registry);
        }

        private void SaveRegistryToState()
        {
            var registry = GetCurrentRegistry();

            if (IsHiddenCursorRegistry(registry))
            {
                CursorRegistrySnapshot = GetDefaultRegistry();
                Logger.Debug($"Failed to take cursor registry snapshot since hidden cursor style is used, reverting to default cursor style");
                SetCursorVisibility(true);
                return;
            }

            CursorRegistrySnapshot = registry;
        }

        private static List<CursorRegistryValue> GetCurrentRegistry()
        {
            using (var cursorKey = Registry.CurrentUser.OpenSubKey(CURSORS_REGISTRY_SUB_KEY))
            {
                var valueNames = cursorKey.GetValueNames();

                return CURSOR_REGISTRY_NAMES
                    .Select(name =>
                    {
                        var value = cursorKey.GetValue(name);
                        return value != null ? new CursorRegistryValue(name, value.ToString()) : null;
                    })
                    .OfType<CursorRegistryValue>()
                    .ToList();
            }
        }

        private static void SetCursorRegistry(List<CursorRegistryValue> registryValues)
        {
            using (var cursorKey = Registry.CurrentUser.OpenSubKey(CURSORS_REGISTRY_SUB_KEY, true))
            {
                registryValues.ForEach(value => cursorKey.SetValue(value.Name, value.Path));
            }
        }

        private static void ApplyCursorSettings()
        {
            SystemParametersInfo(SPI_SETCURSORS, 0, 0, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        private static List<CursorRegistryValue> GetDefaultRegistry()
        {
            return CURSOR_REGISTRY_NAMES
                .Select(name => new CursorRegistryValue(name, ""))
                .ToList();
        }

        private static List<CursorRegistryValue> GetHiddenCursorRegistry()
        {
            return CURSOR_REGISTRY_NAMES
                .Select(name => new CursorRegistryValue(name, SharedSettings.Paths.ResourceEmptyCursor))
                .ToList();
        }

        private static bool IsHiddenCursorRegistry(IEnumerable<CursorRegistryValue> registryValues)
        {
            var emptyCursorFileName = Path.GetFileName(SharedSettings.Paths.ResourceEmptyCursor);

            return registryValues.Any(value => value.Path.Contains(emptyCursorFileName));
        }
    }
}
