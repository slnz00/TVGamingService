using System;

namespace BackgroundService.Source.Services.State.Components
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StateEntry : Attribute
    {
        public readonly string Key;
        public readonly Type Type;

        public StateEntry(string key, Type type)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"State key cannot be empty");
            }

            Key = key;
            Type = type;
        }

        public StateEntry(string key) : this(key, null)
        { }
    }
}
