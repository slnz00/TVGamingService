using Core.Utils;
using System;
using System.IO;
using System.Diagnostics;

namespace Setup
{
    internal class Program
    {
        private static readonly string PATH_APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string PATH_STARTUP_SHORTCUT = Path.Combine(PATH_APPDATA, @"Microsoft\Windows\Start Menu\Programs\Startup\BackgroundService.lnk");

        // TODO: Move paths to SharedConfig, use paths from Core

        static void PrintSoundDeviceNames()
        {
            Console.WriteLine("\nSetup config.json based on available these sound devices:\n");

            // TODO...
        }

        static void PrintDisplayDevicePaths()
        {
            Console.WriteLine("\nSetup config.json based on available these display devices:\n");

            // TODO...
        }

        static void CreateConfigFiles()
        {
            // TODO... Write defaults to json

            Console.WriteLine();
        }

        static void SetupStartupShortcut()
        {
            Console.WriteLine("Creating a shortcut to start the background service with Windows...");

            if (System.IO.File.Exists(PATH_STARTUP_SHORTCUT)) {
                Console.WriteLine("Shortcut already exists, skipping...");
                return;
            }

            OSUtils.CreateShortcut(PATH_STARTUP_SHORTCUT, ""); // TODO: PATH_STARTUP_SCRIPT
        }

        static void OpenConfigFile()
        {
            Process.Start("notepad.exe", ""); // TODO: PATH_CONFIG
        }

        static void Main(string[] args)
        {
            try
            {
                CreateConfigFiles();
                SetupStartupShortcut();
                PrintSoundDeviceNames();
                PrintDisplayDevicePaths();
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
