using System;
using System.Runtime.InteropServices;

namespace BackgroundService.Source.Services.System.API
{
    public static class RestartManagerApi
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public long ProcessStartTime;
        }

        [Flags]
        public enum RM_SHUTDOWN_TYPE : uint
        {
            RmForceShutdown = 0x1,
            RmShutdownOnlyRegistered = 0x10
        }

        public delegate void RM_WRITE_STATUS_CALLBACK(UInt32 nPercentComplete);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        public static extern int RmStartSession(out IntPtr pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        public static extern int RmEndSession(IntPtr pSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        public static extern int RmRegisterResources(IntPtr pSessionHandle, UInt32 nFiles, string[] rgsFilenames, UInt32 nApplications, RM_UNIQUE_PROCESS[] rgApplications, UInt32 nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        public static extern int RmShutdown(IntPtr pSessionHandle, RM_SHUTDOWN_TYPE lActionFlags, RM_WRITE_STATUS_CALLBACK fnStatus);

        [DllImport("rstrtmgr.dll")]
        public static extern int RmRestart(IntPtr pSessionHandle, int dwRestartFlags, RM_WRITE_STATUS_CALLBACK fnStatus);

        [DllImport("kernel32.dll")]
        public static extern bool GetProcessTimes(IntPtr hProcess, out long lpCreationTime, out long lpExitTime, out long lpKernelTime, out long lpUserTime);
    }
}
