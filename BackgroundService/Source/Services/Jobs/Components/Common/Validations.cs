using Core.Utils;
using System;

namespace BackgroundService.Source.Services.Jobs.Components.Common
{
    internal static class Validations
    {
        public static void ValidateEnumValue<TEnum>(string propertyName, string value)
            where TEnum : Enum
        {
            var allowedValues = EnumUtils.GetNames<TEnum>();
            if (!allowedValues.Contains(value))
            {
                throw new ArgumentException($"Unknown enum value for {propertyName}: {value}");
            }
        }

        public static void ValidateNotEmptyOrNull(string propertyName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}
