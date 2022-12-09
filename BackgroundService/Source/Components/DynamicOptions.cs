using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BackgroundService.Source.Components
{
    internal abstract class DynamicOptions
    {
        private readonly object options;
        private readonly Dictionary<Type, object> jsonOptionsCache = new Dictionary<Type, object>();

        protected DynamicOptions(object options)
        {
            this.options = options;
        }

        protected T GetOptions<T>()
        {
            if (options is JObject)
            {
                return GetJSONOptions<T>();
            }

            return (T)options;
        }

        private T GetJSONOptions<T>()
        {
            var cached = jsonOptionsCache.TryGetValue(typeof(T), out object optionsFromCache);
            if (cached)
            {
                return (T)optionsFromCache;
            }

            var convertedOptions = ((JObject)options).ToObject<T>();
            jsonOptionsCache[typeof(T)] = convertedOptions;

            return convertedOptions;
        }
    }
}
