using System;
using System.Linq;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using BackgroundService.Source.Services.State.Components;
using Core.Components;
using Core.Utils;

using static Core.WinAPI.AudioAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class AudioService : Service
    {
        private readonly object threadLock = new object();

        private readonly PolicyConfigClient policyConfig = new PolicyConfigClient();
        private readonly MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();

        private ManagedTask setDevicesTask = null;

        public AudioService(ServiceProvider services) : base(services) { }

        public bool BackupAudioSettings()
        {
            try
            {
                Logger.Info("Creating backup snapshot from current audio settings");

                setDevicesTask?.Cancel();

                var inputIds = new AudioSettingsSnapshot.DeviceIDs()
                {
                    Multimedia = GetDefaultAudioDevice(EDataFlow.eCapture, ERole.eMultimedia)?.ID,
                    Communication = GetDefaultAudioDevice(EDataFlow.eCapture, ERole.eCommunications)?.ID,
                };
                var outputIds = new AudioSettingsSnapshot.DeviceIDs()
                {
                    Multimedia = GetDefaultAudioDevice(EDataFlow.eRender, ERole.eMultimedia)?.ID,
                    Communication = GetDefaultAudioDevice(EDataFlow.eRender, ERole.eCommunications)?.ID,
                };

                var snapshot = new AudioSettingsSnapshot()
                {
                    DefaultInputIDs = inputIds,
                    DefaultOutputIDs = outputIds
                };

                Services.State.Set(States.AudioSettingsSnapshot, snapshot);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to backup audio settings: {ex}");

                return false;
            }
        }

        public bool RestoreAudioSettings()
        {
            try
            {
                Logger.Info("Restoring audio settings from snapshot");

                setDevicesTask?.Cancel();

                var snapshot = Services.State.Get<AudioSettingsSnapshot>(States.AudioSettingsSnapshot);

                if (snapshot == null)
                {
                    Logger.Error("Failed to restore audio settings: Snapshot not found in state");

                    return false;
                }

                SetDefaultAudioDevicePolicy(snapshot.DefaultInputIDs?.Communication, EDataFlow.eCapture, ERole.eCommunications);
                SetDefaultAudioDevicePolicy(snapshot.DefaultInputIDs?.Multimedia, EDataFlow.eCapture, ERole.eMultimedia);
                SetDefaultAudioDevicePolicy(snapshot.DefaultOutputIDs?.Communication, EDataFlow.eRender, ERole.eCommunications);
                SetDefaultAudioDevicePolicy(snapshot.DefaultOutputIDs?.Multimedia, EDataFlow.eRender, ERole.eMultimedia);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to restore audio settings: {ex}");

                return false;
            }
        }

        public void SetDefaultAudioDevices(string inputDeviceName, string outputDeviceName)
        {
            lock (threadLock)
            {
                setDevicesTask?.Cancel();

                setDevicesTask = ManagedTask.Run(async (ctx) =>
                {
                    var inputSet = string.IsNullOrWhiteSpace(inputDeviceName);
                    var outputSet = string.IsNullOrWhiteSpace(outputDeviceName);
                    var timeout = InternalSettings.TIMEOUT_SET_DEFAULT_AUDIO_DEVICE;
                    var tries = timeout / 1000 + 1;

                    if (inputSet && outputSet)
                    {
                        Logger.Info("No input and output device names has been provided for setting default audio devices, skipping...");

                        return;
                    }

                    bool SetDefaultDevice(string deviceName, EDataFlow dataFlow)
                    {
                        var device = GetAudioDeviceByName(deviceName, dataFlow);
                        var type = dataFlow == EDataFlow.eCapture ? "input" : "output";

                        Logger.Info($"Trying to set default {type} audio device to: {deviceName}");

                        if (device == null)
                        {
                            return false;
                        }

                        SetDefaultAudioDevicePolicy(device, ERole.eCommunications);
                        SetDefaultAudioDevicePolicy(device, ERole.eMultimedia);

                        Logger.Info($"Default {type} audio device has been successfully set to: {deviceName}");

                        return true;
                    }

                    while (tries > 0)
                    {
                        lock (threadLock)
                        {
                            if (!inputSet)
                            {
                                inputSet = SetDefaultDevice(inputDeviceName, EDataFlow.eCapture);
                            }
                            if (!outputSet)
                            {
                                outputSet = SetDefaultDevice(outputDeviceName, EDataFlow.eRender);
                            }
                            if (inputSet && outputSet)
                            {
                                return;
                            }
                        }

                        tries--;
                        await ctx.Delay(1000);
                    }

                    if (!inputSet)
                    {
                        Logger.Error($"The timeout for setting the default input audio device has been exceeded: {inputDeviceName}");
                    }
                    if (!outputSet)
                    {
                        Logger.Error($"The timeout for setting the default output audio device has been exceeded: {outputDeviceName}");
                    }
                });
            }
        }

        public void StopMedia()
        {
            ushort VK_MEDIA_STOP = 0xB2;

            var inputs = new Input[] {
                new Input()
                {
                    type = InputType.Keyboard,
                    union = new InputUnion()
                    {
                        keyboard = new KeyboardInput()
                        {
                            wVk = VK_MEDIA_STOP,
                            wScan = 0,
                            dwFlags = KeyEventF.KeyDown,
                            time = 0,
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        private MMDevice GetAudioDeviceByName(string deviceName, EDataFlow dataFlow)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    return null;
                }

                var devices = deviceEnumerator.EnumerateAudioEndPoints(dataFlow, EDeviceState.DEVICE_STATE_ACTIVE);

                return devices
                    .Where(dev => dev.FriendlyName == deviceName)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get audio device by name ({deviceName}): {ex}");

                return null;
            }
        }

        private MMDevice GetAudioDeviceByID(string deviceId, EDataFlow dataFlow)
        {
            try
            {
                if (deviceId == null)
                {
                    return null;
                }

                var devices = deviceEnumerator.EnumerateAudioEndPoints(dataFlow, EDeviceState.DEVICE_STATE_ACTIVE);

                return devices
                    .Where(dev => dev.ID == deviceId)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get audio device by id ({deviceId}): {ex}");

                return null;
            }
        }

        private MMDevice GetDefaultAudioDevice(EDataFlow dataFlow, ERole role)
        {
            try
            {
                return deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role);
            }
            catch (Exception ex)
            {
                var dataFlowName = EnumUtils.GetName(dataFlow);
                var roleName = EnumUtils.GetName(role);

                Logger.Error($"Failed to get default audio device for ({dataFlowName} - {roleName}): {ex}");

                return null;
            }
        }

        private void SetDefaultAudioDevicePolicy(MMDevice device, ERole role)
        {
            var roleName = EnumUtils.GetName(role);

            try
            {
                policyConfig.SetDefaultEndpoint(device.ID, role);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to set default audio device ({device.FriendlyName}) with role ({roleName}): {ex}");
            }
        }

        private void SetDefaultAudioDevicePolicy(string deviceId, EDataFlow dataFlow, ERole role)
        {
            var roleName = EnumUtils.GetName(role);

            try
            {
                var device = GetAudioDeviceByID(deviceId, dataFlow);

                if (device == null)
                {
                    Logger.Error($"Failed to set default audio device ({deviceId}) with role ({roleName}): Device is unavailable");

                    return;
                }

                policyConfig.SetDefaultEndpoint(device.ID, role);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to set default audio device ({deviceId}) with role ({roleName}): {ex}");
            }
        }
    }
}
