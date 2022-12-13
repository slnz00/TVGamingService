using Core.Utils;
using System;
using System.Collections.Generic;

namespace Core.Components
{
    public class EventListener<TEventID, TEventAction>
        where TEventID : Enum
    {
        public readonly uint Id;
        public readonly TEventID EventId;
        public readonly TEventAction Action;

        public EventListener(uint id, TEventID eventId, TEventAction action)
        {
            Id = id;
            EventId = eventId;
            Action = action;
        }
    }

    public class EventListenerRegistry<TEventID, TEventAction> where TEventID : Enum
    {
        private readonly object threadLock = new object();

        private uint listenerIdCounter = 0;

        private Dictionary<uint, EventListener<TEventID, TEventAction>> listenersById = new Dictionary<uint, EventListener<TEventID, TEventAction>>();
        private Dictionary<TEventID, List<EventListener<TEventID, TEventAction>>> listenersByEventId = new Dictionary<TEventID, List<EventListener<TEventID, TEventAction>>>();

        public EventListenerRegistry()
        {
            Initialize();
        }

        public EventListener<TEventID, TEventAction> GetListenerById(uint id)
        {
            lock (threadLock)
            {
                bool listenerExists = listenersById.TryGetValue(id, out EventListener<TEventID, TEventAction> listener);

                return listenerExists ? listener : null;
            }
        }

        public List<EventListener<TEventID, TEventAction>> GetListenersByEventId(TEventID EventId)
        {
            lock (threadLock)
            {
                return CollectionUtils.CloneList(listenersByEventId[EventId]);
            }
        }

        public EventListener<TEventID, TEventAction> AddListener(TEventID eventId, TEventAction action)
        {
            lock (threadLock)
            {
                var listener = new EventListener<TEventID, TEventAction>(listenerIdCounter++, eventId, action);

                listenersById[listener.Id] = listener;
                listenersByEventId[listener.EventId].Add(listener);

                return listener;
            }
        }

        public void RemoveListener(uint id)
        {
            lock (threadLock) {
                bool listenerExists = listenersById.TryGetValue(id, out EventListener<TEventID, TEventAction> listener);
                if (!listenerExists)
                {
                    return;
                }

                var listenersForEvent = listenersByEventId[listener.EventId];

                var listenerIndex = listenersForEvent.IndexOf(listener);
                if (listenerIndex != -1)
                {
                    listenersForEvent.RemoveAt(listenerIndex);
                }
            }
        }

        public void Clear()
        {
            lock (threadLock)
            {
                listenersById.Clear();
                listenersByEventId.Clear();

                Initialize();
            }
        }

        private void Initialize()
        {
            var eventIds = Enum.GetValues(typeof(TEventID));

            foreach (var eventId in eventIds)
            {
                listenersByEventId[(TEventID)eventId] = new List<EventListener<TEventID, TEventAction>>();
            }
        }
    }
}
