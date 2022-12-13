using System.Linq;

namespace BackgroundService.Source.Services.Jobs.Components.Common
{
    internal static class Filters
    {
        public static bool MatchesWithFilter<T>(T target, T filter)
            where T : class
        {
            if (filter == null)
            {
                return true;
            }
            if (target == null)
            {
                return false;
            }

            var type = typeof(T);

            return type.GetProperties().All(prop =>
            {
                var filterValue = prop.GetValue(filter);
                var targetValue = prop.GetValue(target);

                if (filterValue == null)
                {
                    return true;
                }

                return filterValue.Equals(targetValue);
            });
        }
    }
}
