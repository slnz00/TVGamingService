using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TVGamingService
{
    internal static class Utils
    {
        public static string GetFullPath(string relativePath) {
            string execDir = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            return $"{execDir}/{relativePath}";
        }

        public static void CloseProcess(string processName) {
            Process[] playniteProcesses = Process.GetProcessesByName(processName);
            foreach (Process process in playniteProcesses)
            {
                if (!process.CloseMainWindow())
                {
                    process.Kill();
                }
                process.Dispose();
            }
        }

        public static void StartProcess(
            string path,
            string args = "", 
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
            bool waitForExit = false,
            Action<ProcessStartInfo> customizeStartInfo = null
        ) {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = windowStyle == ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.FileName = path;
            startInfo.WindowStyle = windowStyle;
            startInfo.Arguments = args;

            if (customizeStartInfo != null) {
                customizeStartInfo(startInfo);
            }

            try
            {
                Process proc = Process.Start(startInfo);
                if (waitForExit)
                {
                    using (proc)
                    {
                        proc.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start process: {path}, {ex.Message}");
            }
        }
    }
}
