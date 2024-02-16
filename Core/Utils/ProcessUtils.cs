using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Core.Utils
{
    public static class ProcessUtils
    {
        public static void InteractWithProcess(string processName, Action<Process> action)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                action(process);
                process.Dispose();
            }
        }

        public static void InteractWithProcess(int processId, Action<Process> action)
        {
            using (var process = Process.GetProcessById(processId))
            {
                action(process);

                process.Dispose();
            }
        }

        public static List<int> GetProcessIdsByProcessName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            var processIds = processes
                .Select((process) =>
                {
                    return process.Id;
                }).
                ToList();

            Array.ForEach(processes, process => process.Dispose());

            return processIds;
        }

        public static void CloseProcess(string processName, bool forceKill = false)
        {
            InteractWithProcess(processName, (process) =>
            {
                if (!process.CloseMainWindow() || forceKill)
                {
                    process.Kill();
                }
            });
        }

        public static void CloseProcess(int processId, bool forceKill = false)
        {
            InteractWithProcess(processId, (process) =>
            {
                if (!process.CloseMainWindow() || forceKill)
                {
                    process.Kill();
                }
            });
        }

        public static void StartProcess(
            string path,
            string args = "",
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
            bool waitForExit = false,
            Action<ProcessStartInfo> customizeStartInfo = null
        )
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = windowStyle == ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                FileName = path,
                WindowStyle = windowStyle,
                Arguments = args
            };

            customizeStartInfo?.Invoke(startInfo);

            Process proc = Process.Start(startInfo)
                ?? throw new NullReferenceException("Failed to start process");

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
