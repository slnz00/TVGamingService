using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;

namespace BackgroundService.Source.Services.System
{
    internal class CursorService : Service
    {
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);

        private const int SPI_SETCURSORS = 0x0057;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        private static readonly string EMPTY_CURSOR_FILE_NAME = InternalSettings.CURSOR_EMPTY_FILE_NAME;
        private static readonly string EMPTY_CURSOR_FILE_PATH = InternalSettings.PATH_EMPTY_CURSOR;
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

        private List<CursorRegistryValue> visibleCursorRegistrySnapshot;
        private bool cursorVisibility = true;

        public bool CursorVisibility => cursorVisibility;

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
            LoadRegistrySnapshot();
        }

        public void SetCursorVisibility(bool visible)
        {
            Logger.Info($"Setting cursor visibility to: {visible}");

            var cursorValues = visible ? visibleCursorRegistrySnapshot : GetHiddenCursorRegistryValues();

            SetCursorRegistry(cursorValues);
            cursorVisibility = visible;

            ApplyCursorSettings();
        }

        private void LoadRegistrySnapshot()
        {
            var currentValues = GetCurrentRegistryValues();

            // Make sure registry snapshot is not in an empty state to prevent permanently hidden cursors:
            if (IsHiddenCursorRegistry(currentValues))
            {
                visibleCursorRegistrySnapshot = GetDefaultCursorRegistryValues();
                Logger.Debug($"Failed to take cursor registry snapshot since hidden cursor style is used, reverting to default cursor style");
                SetCursorVisibility(true);
                return;
            }

            visibleCursorRegistrySnapshot = currentValues;
        }

        private List<CursorRegistryValue> GetCurrentRegistryValues()
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

        private void SetCursorRegistry(List<CursorRegistryValue> registryValues)
        {
            using (var cursorKey = Registry.CurrentUser.OpenSubKey(CURSORS_REGISTRY_SUB_KEY, true))
            {
                registryValues.ForEach(value => cursorKey.SetValue(value.Name, value.Path));
            }
        }

        private void ApplyCursorSettings()
        {
            SystemParametersInfo(SPI_SETCURSORS, 0, 0, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        private List<CursorRegistryValue> GetDefaultCursorRegistryValues()
        {
            return CURSOR_REGISTRY_NAMES
                .Select(name => new CursorRegistryValue(name, ""))
                .ToList();
        }

        private List<CursorRegistryValue> GetHiddenCursorRegistryValues()
        {
            return CURSOR_REGISTRY_NAMES
                .Select(name => new CursorRegistryValue(name, EMPTY_CURSOR_FILE_PATH))
                .ToList();
        }

        private bool IsHiddenCursorRegistry(IEnumerable<CursorRegistryValue> registryValues)
        {
            return registryValues.Any(value => value.Path.Contains(EMPTY_CURSOR_FILE_NAME));
        }
    }
}
