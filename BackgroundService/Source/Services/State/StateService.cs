using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.State.Components;
using Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackgroundService.Source.Services.State
{
    internal class StateService : Service
    {
        private readonly object threadLock = new object();

        private bool readOnly;

        private Dictionary<string, object> storage;

        public bool ReadOnly => readOnly;

        public StateService(ServiceProvider services, bool readOnly = false) : base(services)
        {
            this.readOnly = readOnly;
        }

        protected override void OnInitialize()
        {
            lock (threadLock)
            {
                ValidateStatesEnum();
                LoadStorageFromFile();
                CleanupStorage();
            }
        }

        public T Get<T>(States state)
        {
            lock (threadLock)
            {
                ValidateInitialized();
                ValidateStateType<T>(state);

                var entry = GetStateEntry(state);
                var exists = storage.TryGetValue(entry.Key, out var value);

                if (!exists)
                {
                    return default;
                }

                if (value is JToken)
                {
                    return ((JToken)value).ToObject<T>();
                }

                return (T)value;
            }
        }

        public void Set<T>(States state, T value)
        {
            lock (threadLock)
            {
                ValidateInitialized();
                ValidateStateType<T>(state);

                if (ReadOnly)
                {
                    throw new InvalidOperationException("'Set' method cannot be called when the service is in read-only mode");
                }

                var entry = GetStateEntry(state);

                storage[entry.Key] = value;

                SaveStorageToFile();
            }
        }

        private void ValidateInitialized()
        {
            if (storage == null)
            {
                throw new InvalidOperationException("Uninitialized storage, call 'Initialize' method first");
            }
        }

        private StateEntry GetStateEntry(States state)
        {
            var entry = (StateEntry)state
                .GetType()
                .GetField(state.ToString())
                .GetCustomAttributes(typeof(StateEntry), false)
                .FirstOrDefault();

            return entry;
        }

        private void ValidateStateType<T>(States state)
        {
            var entry = GetStateEntry(state);

            if (entry.Type == null)
            {
                return;
            }

            var stateName = EnumUtils.GetName(state);
            var usedType = typeof(T);
            var invalidType = entry.Type.Equals(usedType);

            if (!invalidType)
            {
                throw new InvalidOperationException($"State: '{stateName}' cannot be used with type: '{usedType.FullName}'");
            }
        }

        private void ValidateStatesEnum()
        {
            var fields = EnumUtils.GetNames<States>();
            var usedKeys = new List<string>();

            foreach (var field in fields)
            {
                var state = EnumUtils.GetValue<States>(field);
                var entry = GetStateEntry(state);

                if (entry == null)
                {
                    throw new InvalidOperationException($"'States' enum field: '{field}' does not have a 'StateEntry' attribute");
                }
                if (usedKeys.Contains(entry.Key))
                {
                    throw new ArgumentException($"'States' enum field: '{field}', specified key is already in use: '{entry.Key}'");
                }

                usedKeys.Add(entry.Key);
            }
        }

        private void CleanupStorage()
        {
            var availableKeys = EnumUtils
                .GetValues<States>()
                .Select(state => GetStateEntry(state))
                .Select(entry => entry.Key);

            var unusedKeys = storage.Keys
                .Where(key => !availableKeys.Contains(key))
                .ToArray();

            foreach (var key in unusedKeys) {
                storage.Remove(key);
            }
        }

        private void SaveStorageToFile()
        {
            FSUtils.EnsureFileDirectory(InternalSettings.PATH_DATA_STATES);

            var statesJson = JsonConvert.SerializeObject(storage);

            File.WriteAllText(InternalSettings.PATH_DATA_STATES, statesJson, Encoding.Default);
        }

        private void LoadStorageFromFile()
        {
            FSUtils.EnsureFileDirectory(InternalSettings.PATH_DATA_STATES);

            var statesJsonExists = File.Exists(InternalSettings.PATH_DATA_STATES);
            if (!statesJsonExists)
            {
                storage = new Dictionary<string, object>();
                return;
            }

            string statesJson = File.ReadAllText(InternalSettings.PATH_DATA_STATES, Encoding.Default);

            storage = JsonConvert.DeserializeObject<Dictionary<string, object>>(statesJson);
        }
    }
}
