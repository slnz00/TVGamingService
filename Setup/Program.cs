using Core.Utils;
using NAudio.CoreAudioApi;
using System;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using System.Diagnostics;

namespace Setup
{
    internal class Program
    {
        private static readonly string PATH_APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string PATH_BACKGROUND_SERVICE = FSUtils.GetAbsolutePath("BackgroundService.exe");
        private static readonly string PATH_CONFIG = FSUtils.GetAbsolutePath("config.json");
        private static readonly string PATH_EXAMPLE_CONFIG = FSUtils.GetAbsolutePath("config.example.json");
        private static readonly string PATH_GAME_CONFIGS = FSUtils.GetAbsolutePath("game-configs.json");
        private static readonly string PATH_EXAMPLE_GAME_CONFIGS = FSUtils.GetAbsolutePath("game-configs.example.json");
        private static readonly string PATH_JOBS_CONFIG = FSUtils.GetAbsolutePath("job.config.json");
        private static readonly string PATH_EXAMPLE_JOBS_CONFIG = FSUtils.GetAbsolutePath("jobs.config.example.json");
        private static readonly string PATH_STARTUP_SHORTCUT = Path.Combine(PATH_APPDATA, @"Microsoft\Windows\Start Menu\Programs\Startup\BackgroundService.lnk");

        static void PrintSoundDeviceNames()
        {
            Func<MMDevice, string> getDeviceName = (MMDevice device) =>
            {
                const int DEVICE_NAME_PROP_ID = 2;

                for (int i = 0; i < device.Properties.Count; i++)
                {
                    var property = device.Properties[i];

                    if (property.Key.propertyId == DEVICE_NAME_PROP_ID)
                    {
                        return property.Value as string;
                    }
                }

                return null;
            };

            Console.WriteLine("\nSetup config.json based on available these sound devices:\n");

            new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All)
                .Select(getDeviceName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList()
                .ForEach(name => Console.WriteLine(name));
        }

        static void CreateConfigFiles()
        {
            if (!System.IO.File.Exists(PATH_CONFIG))
            {
                Console.WriteLine("Creating default config file...");
                System.IO.File.Copy(PATH_EXAMPLE_CONFIG, PATH_CONFIG);
            }

            if (!System.IO.File.Exists(PATH_GAME_CONFIGS))
            {
                Console.WriteLine("Creating default game configs file...");
                System.IO.File.Copy(PATH_EXAMPLE_GAME_CONFIGS, PATH_GAME_CONFIGS);
            }

            if (!System.IO.File.Exists(PATH_JOBS_CONFIG))
            {
                Console.WriteLine("Creating default jobs config file...");
                System.IO.File.Copy(PATH_EXAMPLE_JOBS_CONFIG, PATH_JOBS_CONFIG);
            }

            Console.WriteLine();
        }

        static void SetupStartupShortcut()
        {
            Console.WriteLine("Creating a shortcut to start the background service with Windows...");

            if (System.IO.File.Exists(PATH_STARTUP_SHORTCUT)) {
                Console.WriteLine("Shortcut already exists, skipping...");
                return;
            }

            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(PATH_STARTUP_SHORTCUT);
            shortcut.TargetPath = PATH_BACKGROUND_SERVICE;
            shortcut.Save();
        }

        static void OpenConfigFile()
        {
            Process.Start("notepad.exe", PATH_CONFIG);
        }

        static void Main(string[] args)
        {
            try
            {
                CreateConfigFiles();
                SetupStartupShortcut();
                PrintSoundDeviceNames();
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
