using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Globalization;
using System.Management;

namespace Core.Utils
{
    public static class OSUtils
    {
        public class WindowsVersion {
            public readonly string version;
            public readonly double buildNumber;

            public WindowsVersion(string version, double buildNumber) {
                this.version = version;
                this.buildNumber = buildNumber;
            }
        }

        public static bool IsWindows11(string windowsVersion)
        {
            return windowsVersion.ToLower().Contains("windows 11");
        }

        public static double GetCurrentWindowsBuildNumber()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var mainBuild = registryKey.GetValue("CurrentBuildNumber").ToString();
            var subBuild = registryKey.GetValue("UBR").ToString();

            return double.Parse($"{mainBuild}.{subBuild}", CultureInfo.InvariantCulture.NumberFormat);
        }

        public static string GetCurrentWindowsVersionName()
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
