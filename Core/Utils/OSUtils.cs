using IWshRuntimeLibrary;
using System.Management;

namespace Core.Utils
{
    public static class OSUtils
    {
        public static bool IsWindows11(string windowsVersion)
        {
            return windowsVersion.ToLower().Contains("windows 11");
        }

        public static string GetCurrentWindowsVersion()
        {
            string result = "";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                ManagementObjectCollection information = searcher.Get();
                if (information != null)
                {
                    foreach (ManagementObject obj in information)
                    {
                        result = obj["Caption"].ToString() + " - " + obj["OSArchitecture"].ToString();
                    }
                }
                result = result.Replace("NT 5.1.2600", "XP");
                result = result.Replace("NT 5.2.3790", "Server 2003");

                return result;
            }
        }

        public static void CreateShortcut(string shortcutPath, string targetPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.Save();
        }
    }
}
