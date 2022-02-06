using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Threading;

namespace TVGamingService
{
    internal static class SoundDeviceManager
    {
        private static readonly object threadLock = new object();

        private static Thread setDefaultThread = null;

        public static void SetDefaultSoundDevice(string deviceName, uint timeout = 0)
        {
            lock (threadLock) {
                if (setDefaultThread != null && setDefaultThread.IsAlive) {
                    setDefaultThread.Abort();
                }

                ThreadStart threadHandler = () => SetDefaultSoundDeviceLoop(deviceName, timeout);
                setDefaultThread = new Thread(threadHandler);
                setDefaultThread.Start();
            }
        }

        public static string GetDefaultSoundDeviceName() {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

            try
            {
                const int DEVICE_NAME_PROP_ID = 2;

                for (int i = 0; i < device.Properties.Count; i++) {
                    var property = device.Properties[i];

                    bool isDeviceName = property.Key.propertyId == DEVICE_NAME_PROP_ID;
                    if (isDeviceName) {
                        return property.Value as string;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static void SetDefaultSoundDeviceLoop(string targetDeviceName, uint timeout)
        {
            uint loopCount = timeout / 1000 + 1;

            while (loopCount > 0)
            {
                TryToSetDefaultDevice(targetDeviceName);
                Thread.Sleep(200);

                string defaultDeviceName = GetDefaultSoundDeviceName();
                bool failedToRetrieveDefaultDevice = defaultDeviceName == null;
                bool deviceSuccesfullySet = defaultDeviceName == targetDeviceName;

                Console.WriteLine($"AudioChange: Current: {defaultDeviceName} Target: {targetDeviceName}");

                if (failedToRetrieveDefaultDevice || deviceSuccesfullySet)
                {
                    break;
                }

                Thread.Sleep(800);
                loopCount--;
            }
        }

        private static void TryToSetDefaultDevice(string deviceName)
        {
            string nircmdPath = Utils.GetFullPath("deps/nircmd.exe");
            Utils.StartProcess(nircmdPath, $"setdefaultsounddevice \"{deviceName}\"", ProcessWindowStyle.Hidden, true);
        }
    }
}
