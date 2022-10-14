using System;
using System.Diagnostics;
using TVGamingService.Source.Providers;

namespace TVGamingService.Source.Utils
{
    internal static class ProcessUtils
    {
        private static readonly LoggerProvider Logger = new LoggerProvider(typeof(ProcessUtils).Name);

        public static void CloseProcess(string processName, bool forceKill = false)
        {
            Process[] playniteProcesses = Process.GetProcessesByName(processName);

            foreach (Process process in playniteProcesses)
            {
                if (!process.CloseMainWindow() || forceKill)
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
        )
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = windowStyle == ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.FileName = path;
            startInfo.WindowStyle = windowStyle;
            startInfo.Arguments = args;

            if (customizeStartInfo != null)
            {
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
            catch (Exception e)
            {
                Logger.Error($"Failed to start process: {path}, exception: {e}");
            }
        }
    }
}
