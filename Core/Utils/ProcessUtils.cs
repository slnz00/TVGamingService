using System;
using System.Diagnostics;

namespace Core.Utils
{
    public static class ProcessUtils
    {
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

            Process proc = Process.Start(startInfo);
            if (proc == null) {
                throw new NullReferenceException("Failed to start process");
            }

            if (waitForExit)
            {
                using (proc)
                {
                    proc.WaitForExit();
                }
            }
        }
    }
}
