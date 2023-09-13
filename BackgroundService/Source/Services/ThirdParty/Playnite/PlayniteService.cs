using System;
using System.Diagnostics;
using System.IO;
using BackgroundService.Source.Common;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using BackgroundService.Source.Services.ThirdParty.Playnite.Communication;
using BackgroundService.Source.Services.ThirdParty.Playnite.Communication.Services;
using Core.Components;
using Core.Playnite.Communication.Models;
using Core.Utils;

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

        private readonly EventListenerRegistry<PlayniteEventID, Action<object>> eventListenerRegistry = new EventListenerRegistry<PlayniteEventID, Action<object>>();

        private ProcessWatcher.Events watcherEvents;
        private PlayniteAppService.Events playniteAppEvents;

        private AppCommunicationHost playniteCommunication;
        private ProcessWatcher watcherPlayniteFullscreen;

        private AppConfig configPlayniteFullscreen;
        private AppConfig configPlayniteDesktop;

        public bool IsPlayniteFullscreenOpen => watcherPlayniteFullscreen != null && watcherPlayniteFullscreen.IsProcessOpen;

        public PlayniteService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            configPlayniteFullscreen = Services.Config.GetConfig().ThirdParty.PlayniteFullscreen;
            configPlayniteDesktop = Services.Config.GetConfig().ThirdParty.PlayniteDesktop;

            if (configPlayniteFullscreen == null || configPlayniteDesktop == null)
            {
                throw new NullReferenceException("PlayniteDesktop and PlayniteFullscreen third-party configurations are not defined");
            }

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

            watcherPlayniteFullscreen = new ProcessWatcher(configPlayniteFullscreen.ProcessName, watcherEvents, 1000);
            playniteCommunication = new AppCommunicationHost(playniteAppEvents);

            playniteCommunication.Open();
            watcherPlayniteFullscreen.Start();
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

        public void OpenFullscreenPlaynite()
        {
            Logger.Info("Opening Playnite fullscreen application");

            var playnitePath = FSUtils.GetAbsolutePath(configPlayniteFullscreen.Path);
            var playniteDir = Path.GetDirectoryName(playnitePath);

            ProcessUtils.StartProcess(playnitePath, "", ProcessWindowStyle.Normal, false, (startInfo) =>
            {
                startInfo.WorkingDirectory = playniteDir;
            });
        }

        public void CloseFullscreenPlaynite()
        {
            Logger.Info("Closing Playnite fullscreen application");

            ProcessUtils.CloseProcess(configPlayniteFullscreen.ProcessName);
        }

        public void CloseDesktopPlaynite()
        {
            Logger.Info("Closing Playnite desktop application");

            ProcessUtils.CloseProcess(configPlayniteDesktop.ProcessName);
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
