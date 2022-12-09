﻿using System;
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
            PLAYNITE_OPENED,
            PLAYNITE_CLOSED,
            GAME_STARTING,
            GAME_STOPPED,
        }

        private readonly EventListenerRegistry<PlayniteEventID, Action<object>> eventListenerRegistry = new EventListenerRegistry<PlayniteEventID, Action<object>>();

        private ProcessWatcher.Events watcherEvents;
        private PlayniteAppEventsService.Events playniteAppEvents;

        private AppCommunicationHost playniteCommunication;
        private ProcessWatcher watcher;

        private AppConfig playniteConfig;

        public bool IsPlayniteOpen => watcher != null && watcher.IsProcessOpen;

        public PlayniteService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            playniteConfig = Services.Config.GetConfig().ThirdParty.Playnite;

            watcherEvents = new ProcessWatcher.Events
            {
                OnProcessOpened = () => RunEventListeners(PlayniteEventID.PLAYNITE_OPENED, null),
                OnProcessClosed = () => RunEventListeners(PlayniteEventID.PLAYNITE_CLOSED, null)
            };

            playniteAppEvents = new PlayniteAppEventsService.Events
            {
                OnGameStarting = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GAME_STARTING, gameInfo),
                OnGameStopped = (PlayniteGameInfo gameInfo) => RunEventListeners(PlayniteEventID.GAME_STOPPED, gameInfo),
            };

            watcher = new ProcessWatcher(playniteConfig.ProcessName, watcherEvents, 1000);
            playniteCommunication = new AppCommunicationHost(playniteAppEvents);

            playniteCommunication.Open();
            watcher.Start();
        }

        protected override void OnDispose()
        {
            playniteCommunication.Close();
            watcher.Stop();
        }

        public uint OnPlayniteOpened(Action action) {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.PLAYNITE_OPENED, (_args) => action());
            return listener.Id;
        }

        public uint OnPlayniteClosed(Action action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.PLAYNITE_CLOSED, (_args) => action());
            return listener.Id;
        }

        public uint OnGameStarting(Action<PlayniteGameInfo> action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.GAME_STARTING, (args) => action((PlayniteGameInfo)args));
            return listener.Id;
        }

        public uint OnGameStopped(Action<PlayniteGameInfo> action)
        {
            var listener = eventListenerRegistry.AddListener(PlayniteEventID.GAME_STOPPED, (args) => action((PlayniteGameInfo)args));
            return listener.Id;
        }

        public void RemoveEventListener(UInt32 listenerId)
        {
            eventListenerRegistry.RemoveListener(listenerId);
        }

        public void OpenPlaynite()
        {
            Logger.Debug("Opening Playnite");

            var playnitePath = Path.GetFullPath(playniteConfig.Path);
            var playniteDir = Path.GetDirectoryName(playnitePath);

            ProcessUtils.StartProcess(playnitePath, "", ProcessWindowStyle.Normal, false, (startInfo) =>
            {
                startInfo.WorkingDirectory = playniteDir;
            });
        }

        public void ClosePlaynite()
        {
            Logger.Debug("Closing Playnite");
            
            ProcessUtils.CloseProcess(playniteConfig.ProcessName);
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
