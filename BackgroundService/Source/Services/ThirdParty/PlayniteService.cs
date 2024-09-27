using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Communication.ServiceHosts;
using Core.Components;
using Core.Components.Watchers;
using Core.Models.Configs;
using Core.Models.Playnite;
using Core.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BackgroundService.Source.Services.ThirdParty.Playnite
{
    internal class PlayniteService : Service
    {
        private enum PlayniteEventID
        {
            PlayniteOpened,
            PlayniteClosed,
            GameStarting,
            GameStarted,
            GameStopped,
        }

        private PlayniteAppService PlayniteApp => Services.Communication.Playnite;

        private readonly object threadLock = new object();

        private readonly EventListenerRegistry<PlayniteEventID, Action<object>> eventListenerRegistry = new EventListenerRegistry<PlayniteEventID, Action<object>>();

        private ProcessWatcher.Events watcherEvents;

        private ProcessWatcher watcherPlayniteFullscreen;

        private AppConfig ConfigPlayniteFullscreen => Services.Config.GetConfig().ThirdParty?.PlayniteFullscreen;
        private AppConfig ConfigPlayniteDesktop => Services.Config.GetConfig().ThirdParty?.PlayniteDesktop;

        public bool IsPlayniteFullscreenOpen => watcherPlayniteFullscreen != null && watcherPlayniteFullscreen.IsProcessOpen;

        public PlayniteService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            watcherEvents = new ProcessWatcher.Events
            {
                OnProcessOpened = () => RunEventListeners(PlayniteEventID.PlayniteOpened, null),
                OnProcessClosed = () => RunEventListeners(PlayniteEventID.PlayniteClosed, null)
            };

            PlayniteApp.SetEventHandlers(new PlayniteAppService.Events
            {
                OnGameStarting = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GameStarting, gameInfo),
                OnGameStarted = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GameStarted, gameInfo),
                OnGameStopped = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GameStopped, gameInfo),
            });

            StartWatcher();

            Services.Config.ConfigWatcher.OnChanged(() =>
            {
                lock (threadLock)
                {
                    StopWatcher();
                    StartWatcher();
                }
            });
        }

        protected override void OnDispose()
        {
            watcherPlayniteFullscreen.Stop();
        }

        public uint OnPlayniteOpened(Action action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.PlayniteOpened, (_args) => action());
            return listener.Id;
        }

        public uint OnPlayniteClosed(Action action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.PlayniteClosed, (_args) => action());
            return listener.Id;
        }

        public uint OnGameStarting(Action<PlayniteGameInfo> action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.GameStarting, (args) => action((PlayniteGameInfo)args));
            return listener.Id;
        }

        public uint OnGameStarted(Action<PlayniteGameInfo> action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.GameStarted, (args) => action((PlayniteGameInfo)args));
            return listener.Id;
        }

        public uint OnGameStopped(Action<PlayniteGameInfo> action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.GameStopped, (args) => action((PlayniteGameInfo)args));
            return listener.Id;
        }

        public void RemoveEventListener(uint listenerId)
        {
            eventListenerRegistry.RemoveListener(listenerId);
        }

        public bool IsPlayniteAvailable()
        {
            var validConfigs = new string[]
            {
                ConfigPlayniteFullscreen?.Path,
                ConfigPlayniteFullscreen?.ProcessName,
                ConfigPlayniteDesktop?.Path,
                ConfigPlayniteDesktop?.ProcessName
            }
            .All(val => !string.IsNullOrWhiteSpace(val));

            if (!validConfigs)
            {
                return false;
            }

            var validPaths = new string[]
            {
                ConfigPlayniteFullscreen.Path,
                ConfigPlayniteDesktop.Path
            }
            .All(path => File.Exists(path));

            return validPaths;
        }

        public void OpenFullscreenPlaynite()
        {
            Logger.Info("Opening Playnite fullscreen application");

            var playnitePath = FSUtils.GetAbsolutePath(ConfigPlayniteFullscreen.Path);
            var playniteDir = Path.GetDirectoryName(playnitePath);

            ProcessUtils.StartProcess(playnitePath, "", ProcessWindowStyle.Normal, false, (startInfo) =>
            {
                startInfo.WorkingDirectory = playniteDir;
            });

            CloseSafeModeWindow();
        }

        public void FocusFullscreenPlaynite()
        {
            var currentDesktopId = Services.OS.Desktop.GetCurrentDesktopId();
            var allWindows = Services.OS.Desktop.GetWindowsOnDesktop(currentDesktopId);

            var playniteFullscreenWindow = allWindows
                .FirstOrDefault(win => win.Process.ProcessName == ConfigPlayniteFullscreen.ProcessName);

            if (playniteFullscreenWindow == null)
            {
                return;
            }

            playniteFullscreenWindow.Focus();
        }

        public void CloseFullscreenPlaynite()
        {
            lock (threadLock)
            {
                Logger.Info("Closing Playnite fullscreen application");

                ProcessUtils.CloseProcess(ConfigPlayniteFullscreen.ProcessName, false, TimeSpan.FromSeconds(3));
            }
        }

        public void CloseDesktopPlaynite()
        {
            lock (threadLock)
            {
                Logger.Info("Closing Playnite desktop application");

                ProcessUtils.CloseProcess(ConfigPlayniteDesktop.ProcessName, false, TimeSpan.FromSeconds(3));
            }
        }

        public void CloseSafeModeWindow()
        {
            Func<bool> tryToCloseWindow = () =>
            {
                try
                {
                    var safeModeWindow = Services.OS.Window.FindWindowByName("Startup Error");
                    if (!safeModeWindow.IsValid)
                    {
                        return false;
                    }

                    var isPlayniteWindow = safeModeWindow.Process.ProcessName.ToLower().Contains("playnite");
                    if (!isPlayniteWindow)
                    {
                        return false;
                    }

                    var buttons = Services.OS.Window.GetChildComponents(safeModeWindow, "Button");
                    var noButton = buttons.Count == 2 ? buttons[1] : null;
                    if (noButton == null)
                    {
                        return false;
                    }

                    noButton.Click();

                    return true;
                }
                catch
                {
                    return false;
                }
            };

            ManagedTask.Run(async (ctx) =>
            {
                for (int tries = 0; tries < 10; tries++)
                {
                    if (tryToCloseWindow())
                    {
                        return;
                    }

                    await ctx.Delay(500);
                }
            });
        }

        private void StopWatcher()
        {
            if (watcherPlayniteFullscreen != null)
            {
                watcherPlayniteFullscreen.Stop();
                watcherPlayniteFullscreen = null;
            }
        }

        private void StartWatcher()
        {
            if (ConfigPlayniteFullscreen == null || ConfigPlayniteDesktop == null)
            {
                throw new NullReferenceException("PlayniteDesktop and PlayniteFullscreen third-party configurations are not defined");
            }

            watcherPlayniteFullscreen = new ProcessWatcher(ConfigPlayniteFullscreen.ProcessName, Logger, watcherEvents, 1000);
            watcherPlayniteFullscreen.Start();
        }

        private void RunEventListeners(PlayniteEventID eventId, object args)
        {
            var listeners = eventListenerRegistry.GetListenersByEventId(eventId);

            listeners.ForEach(listener =>
            {
                try
                {
                    listener.Action(args);
                }
                catch (Exception ex)
                {
                    var eventName = EnumUtils.GetName(listener.EventId);
                    Logger.Error($"Failed to run action for playnite event ({eventName}): {ex}");
                }
            });
        }
    }
}
