using Core;
using Core.Configs;
using Core.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Core.WinAPI.AudioAPI;
using static Core.WinAPI.DisplayAPI;

namespace Setup
{
    internal class Program
    {
        private static readonly string PathAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string PathStartupShortcut = Path.Combine(PathAppData, @"Microsoft\Windows\Start Menu\Programs\Startup\GameEnvironmentService.lnk");

        static void PrintAudioDevices()
        {
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();

            var deviceCollection = DevEnum.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATEMASK_ALL);

            var allDevices = deviceCollection
                .Where(dev => !string.IsNullOrWhiteSpace(dev.FriendlyName))
                .ToList();

            var inputDeviceNames = allDevices
                .Where(dev => dev.DataFlow == EDataFlow.eCapture)
                .Select(dev => dev.FriendlyName)
                .GroupBy(name => name)
                .Select(gp => gp.First())
                .OrderBy(name => name);

            var outputDeviceNames = allDevices
                .Where(dev => dev.DataFlow == EDataFlow.eRender)
                .Select(dev => dev.FriendlyName)
                .GroupBy(name => name)
                .Select(gp => gp.First())
                .OrderBy(name => name);

            Console.WriteLine("\nInput audio devices (DeviceName):");

            foreach (var name in inputDeviceNames)
            {
                Console.WriteLine($"\t{name}");
            }

            Console.WriteLine("\nOutput audio devices (DeviceName):");

            foreach (var name in outputDeviceNames)
            {
                Console.WriteLine($"\t{name}");
            }
        }

        static void PrintDisplayDevices()
        {
            string GetFullName(DISPLAYCONFIG_TARGET_DEVICE_NAME nameInfo)
            {
                var baseName = nameInfo.monitorFriendlyDeviceName;
                var devicePath = nameInfo.monitorDevicePath;

                if (string.IsNullOrWhiteSpace(baseName) || string.IsNullOrWhiteSpace(devicePath))
                {
                    return "";
                };

                var devicePathParts = devicePath.Split('#');

                if (devicePathParts.Length < 2)
                {
                    return baseName;
                }

                var modelName = devicePathParts[1];

                return $"{baseName} ({modelName})";
            }

            DISPLAYCONFIG_TARGET_DEVICE_NAME GetTargetNameInfo(DISPLAYCONFIG_PATH_TARGET_INFO targetInfo)
            {
                var targetNameInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME
                {
                    header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                    {
                        adapterId = targetInfo.adapterId,
                        id = targetInfo.id,
                        size = Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                        type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME,
                    }
                };

                DisplayConfigGetDeviceInfo(ref targetNameInfo);

                return targetNameInfo;
            }

            QueryDisplayConfig(
              QUERY_DISPLAY_CONFIG_FLAGS.QDC_ALL_PATHS,
              out var numPaths,
              out var pathArray,
              out var numModes,
              out var modeArray,
              IntPtr.Zero
            );

            var availableTargets = pathArray
                .Select(path => path.targetInfo)
                .Where(targetInfo => targetInfo.targetAvailable == 1)
                .GroupBy(targetInfo => targetInfo.id)
                .Select(group => group.First())
                .ToList();

            Console.WriteLine("Displays (DeviceName - DevicePath):");

            availableTargets.ForEach(target =>
            {
                var nameInfo = GetTargetNameInfo(target);
                var deviceName = GetFullName(nameInfo);
                var devicePath = nameInfo.monitorDevicePath;

                Console.WriteLine($"\t{deviceName} - {devicePath}");
            });
        }

        static void CreateConfigFiles()
        {
            var configExists = File.Exists(SharedSettings.Paths.Config);
            var jobsConfigExists = File.Exists(SharedSettings.Paths.JobsConfig);

            if (configExists && jobsConfigExists)
            {
                return;
            }

            Console.WriteLine("Creating default config files...");

            if (!configExists)
            {
                var config = new Config();

                FSUtils.EnsureFileDirectory(SharedSettings.Paths.Config);
                Config.WriteToFile(config, SharedSettings.Paths.Config);
            }

            if (!jobsConfigExists)
            {
                var jobsConfig = new JobsConfig();

                FSUtils.EnsureFileDirectory(SharedSettings.Paths.JobsConfig);
                JobsConfig.WriteToFile(jobsConfig, SharedSettings.Paths.JobsConfig);
            }
        }

        static void SetupStartupShortcut()
        {
            if (File.Exists(PathStartupShortcut))
            {
                return;
            }

            Console.WriteLine("Creating shortcut for starting background service with Windows...");

            OSUtils.CreateShortcut(PathStartupShortcut, SharedSettings.Paths.ServiceStartupScript);
        }

        static void OpenConfigFile()
        {
            Console.WriteLine("\nSetup Config.json according to the available display and audio devices listed above");

            Process.Start("notepad.exe", SharedSettings.Paths.Config);
        }

        static void Main()
        {
            try
            {
                CreateConfigFiles();
                SetupStartupShortcut();
                PrintDisplayDevices();
                PrintAudioDevices();
                OpenConfigFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
