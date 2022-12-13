using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utils
{
    public static class EnumUtils
    {
        public static T GetValue<T>(string name)
            where T : Enum
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        public static string GetName<T>(T value)
            where T : Enum
        {
            return Enum.GetName(typeof(T), value);
        }

        public static List<int> GetValues<T>()
            where T : Enum
        {
            return Enum
                .GetValues(typeof(T))
                .OfType<T>()
                .Select(x => (int)(object)x)
                .ToList();
        }

        public static List<string> GetNames<T>()
           where T : Enum
        {
            return Enum
                .GetNames(typeof(T))
                .ToList();
        }
    }
}
