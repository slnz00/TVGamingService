using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace TVGamingService
{
    static class Program
    {
        static readonly object threadLock = new object();

        static readonly HotkeyManager hotkeyManager = new HotkeyManager();
        static readonly Config config = Config.LoadFromFile();

        static readonly string PLAYNITE_PROCESS_NAME = "Playnite.FullscreenApp";
        static readonly string EGL_PROCESS_NAME = "EpicGamesLauncher";
        static readonly string STEAM_PROCESS_NAME = "Steam";

        static bool isTvEnvironment = false;
        static Thread playniteWatcherThread = null;

        static void Main()
        {
            hotkeyManager.RegisterAction(HotkeyManager.KeyModifiers.Alt, Keys.NumPad0, () => {
                lock (threadLock) {
                    SwitchEnvironment();
                }
            });

            MessageLoop.Run();
        }

        static void SwitchEnvironment()
        {
            isTvEnvironment = !isTvEnvironment;

            SwitchDisplay();
            SwitchSoundDevice();
            ManageDesktops();
            ManageDS4Windows();
            ManagePlaynite();
            ManageGameStores();
        }

        static void ManageDesktops()
        {
            const string tvDesktopName = "TVGaming";

            if (isTvEnvironment)
            {
                DesktopManager.CreateAndSwitchToDesktop(tvDesktopName);
            }
            else
            {
                DesktopManager.RemoveDesktop(tvDesktopName);
            }

            DesktopManager.ToggleIconsVisiblity();
        }

        static void SwitchDisplay()
        {
            var displays = LegacyDisplayManager.GetDisplays();
            displays[0].Disable();

            var resolution = isTvEnvironment ? config.TV.DisplayResolution : config.PC.DisplayResolution;
            displays = LegacyDisplayManager.GetDisplays();
            displays[1].SetAsPrimary();
            displays[1].SetResolution(resolution.Width, resolution.Height);

            LegacyDisplayManager.SaveDisplaySettings();
        }

        static void SwitchSoundDevice()
        {
            string deviceName = isTvEnvironment ? config.TV.SoundDevice : config.PC.SoundDevice;

            SoundDeviceManager.SetDefaultSoundDevice(deviceName, 30000);
        }

        static void ManagePlaynite()
        {
            Utils.CloseProcess(PLAYNITE_PROCESS_NAME);

            if (isTvEnvironment)
            {
                Utils.StartProcess(config.Playnite.Path);

                // Switch back to desktop:
                Action onPlayniteClosed = () => SwitchEnvironment();
                StartPlayniteWatcher(onPlayniteClosed);
            }
        }

        static void ManageGameStores()
        {
            if (!isTvEnvironment) {
                Utils.CloseProcess(STEAM_PROCESS_NAME, true);
                Utils.CloseProcess(EGL_PROCESS_NAME, true);
            }
        }

        static void ManageDS4Windows()
        {
            Utils.StartProcess(config.DS4Windows.Path, "-command shutdown", ProcessWindowStyle.Hidden, true);

            if (isTvEnvironment)
            {
                Utils.StartProcess(config.DS4Windows.Path);
            }
        }


        static void StartPlayniteWatcher(Action onPlayniteClosed)
        {
            if (playniteWatcherThread != null && playniteWatcherThread.IsAlive)
            {
                playniteWatcherThread.Abort();
            }

            playniteWatcherThread = new Thread(() => {
                while (isTvEnvironment)
                {
                    lock (threadLock)
                    {
                        if (Process.GetProcessesByName(PLAYNITE_PROCESS_NAME).Length == 0)
                        {
                            onPlayniteClosed();
                            break;
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
            playniteWatcherThread.Start();
        }
    }
}
