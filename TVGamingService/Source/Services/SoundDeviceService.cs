using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Threading;
using TVGamingService.Source.Providers;
using TVGamingService.Source.Utils;

namespace TVGamingService.Source.Services
{
    internal class SoundDeviceService : BaseService
    {
        private static readonly string NIRCMD_PATH = InternalSettings.PATH_NIRCMD;
        private static readonly uint DEFAULT_TIMEOUT = InternalSettings.TIMEOUT_DEFAULT_SOUND_DEVICE_SET;

        private readonly object threadLock = new object();

        private Thread setDefaultThread = null;

        public SoundDeviceService(ServiceProvider services) : base(services) { }

        public void SetDefaultSoundDevice(string deviceName) {
            SetDefaultSoundDevice(deviceName, DEFAULT_TIMEOUT);
        }

        public void SetDefaultSoundDevice(string deviceName, uint timeout)
        {
            lock (threadLock)
            {
                if (setDefaultThread != null && setDefaultThread.IsAlive)
                {
                    setDefaultThread.Abort();
                }

                ThreadStart threadHandler = () => SetDefaultSoundDeviceLoop(deviceName, timeout);
                setDefaultThread = new Thread(threadHandler);
                setDefaultThread.Start();
            }
        }

        public string GetDefaultSoundDeviceName()
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

            try
            {
                const int DEVICE_NAME_PROP_ID = 2;

                for (int i = 0; i < device.Properties.Count; i++)
                {
                    var property = device.Properties[i];

                    bool isDeviceName = property.Key.propertyId == DEVICE_NAME_PROP_ID;
                    if (isDeviceName)
                    {
                        return property.Value as string;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to retrieve default sound device: {e}");

                return null;
            }
        }

        private void SetDefaultSoundDeviceLoop(string targetDeviceName, uint timeout)
        {
            uint tries = timeout / 1000 + 1;

            while (tries > 0)
            {
                lock (threadLock)
                {
                    string defaultDeviceName = GetDefaultSoundDeviceName();
                    Logger.Debug($"Trying to change default sound device: {defaultDeviceName} -> {targetDeviceName}");

                    TryToSetDefaultDevice(targetDeviceName);
                    Thread.Sleep(200);

                    string defaultDeviceNameAfterChange = GetDefaultSoundDeviceName();

                    bool failedToRetrieveDefaultDevice = defaultDeviceNameAfterChange == null;
                    if (failedToRetrieveDefaultDevice)
                    {
                        Logger.Error($"Sound device change failed: {defaultDeviceName} -> {targetDeviceName}");
                        return;
                    }

                    bool deviceSuccesfullySet = defaultDeviceNameAfterChange == targetDeviceName;
                    if (deviceSuccesfullySet)
                    {
                        Logger.Info($"Default sound device successfully canged to: {targetDeviceName}");
                        return;
                    }
                }

                Thread.Sleep(800);
                tries--;
            }

            Logger.Error($"Failed to set default sound devicce to: {targetDeviceName} - Timeout");
        }

        private void TryToSetDefaultDevice(string deviceName)
        {
            ProcessUtils.StartProcess(NIRCMD_PATH, $"setdefaultsounddevice \"{deviceName}\"", ProcessWindowStyle.Hidden, true);
        }

        private void ExecNIRCMD(string args)
        {
            Logger.Debug($"Exec NIRCMD, args: {args}");

            ProcessUtils.StartProcess(NIRCMD_PATH, args, ProcessWindowStyle.Hidden, true);
        }
    }
}
