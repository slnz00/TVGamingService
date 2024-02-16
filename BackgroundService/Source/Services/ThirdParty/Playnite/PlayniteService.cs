using BackgroundService.Source.Common;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.ThirdParty.Playnite.Communication;
using BackgroundService.Source.Services.ThirdParty.Playnite.Communication.Services;
using Core.Components;
using Core.Configs;
using Core.Playnite.Communication.Models;
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

        private readonly object threadLock = new object();

        private readonly EventListenerRegistry<PlayniteEventID, Action<object>> eventListenerRegistry = new EventListenerRegistry<PlayniteEventID, Action<object>>();

        private ProcessWatcher.Events watcherEvents;
        private PlayniteAppService.Events playniteAppEvents;

        private AppCommunicationHost playniteCommunication;
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

            playniteAppEvents = new PlayniteAppService.Events
            {
                OnGameStarting = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GameStarting, gameInfo),
                OnGameStarted = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GameStarted, gameInfo),
                OnGameStopped = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GameStopped, gameInfo),
            };

            StartServices();

            Services.Config.ConfigWatcher.OnChanged(() =>
            {
                lock (threadLock)
                {
                    StopServices();
                    StartServices();
                }
            });
        }

        protected override void OnDispose()
        {
            playniteCommunication.Close();
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

        public void RemoveEventListener(UInt32 listenerId)
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
        }

        public void CloseFullscreenPlaynite()
        {
            lock (threadLock)
            {
                Logger.Info("Closing Playnite fullscreen application");

                ProcessUtils.CloseProcess(ConfigPlayniteFullscreen.ProcessName);
            }
        }

        public void CloseDesktopPlaynite()
        {
            lock (threadLock)
            {
                Logger.Info("Closing Playnite desktop application");

                ProcessUtils.CloseProcess(ConfigPlayniteDesktop.ProcessName);
            }
        }

        private void StopServices()
        {
            if (playniteCommunication != null)
            {
                playniteCommunication.Close();
                playniteCommunication = null;
            }

            if (watcherPlayniteFullscreen != null)
            {
                watcherPlayniteFullscreen.Stop();
                watcherPlayniteFullscreen = null;
            }
        }

        private void StartServices()
        {
            if (ConfigPlayniteFullscreen == null || ConfigPlayniteDesktop == null)
            {
                throw new NullReferenceException("PlayniteDesktop and PlayniteFullscreen third-party configurations are not defined");
            }

            watcherPlayniteFullscreen = new ProcessWatcher(ConfigPlayniteFullscreen.ProcessName, watcherEvents, 1000);
            playniteCommunication = new AppCommunicationHost(playniteAppEvents);

            playniteCommunication.Open();
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
