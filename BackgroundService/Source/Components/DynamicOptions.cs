using Newtonsoft.Json.Linq;

namespace BackgroundService.Source.Components
{
    internal abstract class DynamicOptions
    {
        private object options;

        protected DynamicOptions(object options)
        {
            this.options = options;
        }

        protected T GetOptions<T>()
        {
            if (options is JObject) {
                return ((JObject)options).ToObject<T>();
            }

            return (T)options;
        }
    }
}
