using System;

namespace Core.Utils
{
    public static class EnumUtils
    {
        public static T GetValue<T>(string name) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        public static string GetName<T>(T value) where T : Enum
        {
            return Enum.GetName(typeof(T), value);
        }
    }
}
