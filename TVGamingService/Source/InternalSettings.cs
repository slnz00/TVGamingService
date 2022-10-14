using System.Windows.Forms;
using TVGamingService.Source.Models;
using TVGamingService.Source.Utils;

namespace TVGamingService.Source
{
    internal static class InternalSettings
    {
        public static readonly HotkeyDefinition HOTKEY_SWITCH_ENVIRONMENTS = new HotkeyDefinition(KeyModifiers.Alt, Keys.NumPad0);
        public static readonly HotkeyDefinition HOTKEY_TOGGLE_CONSOLE_VISIBILITY = new HotkeyDefinition(KeyModifiers.Alt, Keys.NumPad1);
        public static readonly HotkeyDefinition HOTKEY_RESET_ENVIRONMENT = new HotkeyDefinition(KeyModifiers.Alt, Keys.NumPad9);

        public static readonly string DESKTOP_TV_NAME = "TVGaming";

        public static readonly string PATH_CONFIG = FSUtils.GetAbsolutePath("config.json");
        public static readonly string PATH_VIRTUAL_DESKTOP = FSUtils.GetAbsolutePath("Binaries/vd.exe");
        public static readonly string PATH_NIRCMD = FSUtils.GetAbsolutePath("Binaries/nircmd.exe");

        public static readonly uint TIMEOUT_HOTKEY_ACTION = 1000;
        public static readonly uint TIMEOUT_DEFAULT_SOUND_DEVICE_SET = 25_000;

        public static readonly bool CONSOLE_DEFAULT_VISIBILITY = false;

        public static readonly bool DEBUG_ENABLED = true;
    }
}
