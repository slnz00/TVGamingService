using System;

namespace Core
{
    public static class SharedSettings
    {
        public static class ProcessGuids
        {
            public static readonly Guid BACKGROUND_SERVICE = new Guid("929f0e08-977a-4c90-b825-6823e4f75d5a");
        }

        public static class Playnite
        {
            public static readonly Guid PIPE_GUID = new Guid("4dbc9aba-f0c2-4ba0-a637-5ca12f3a621a");
            public static readonly string PIPE_BASE_URL = $"net.pipe://localhost/TVGamingService-Playnite-{PIPE_GUID}";

            public static string GetServiceAddress<TService>() where TService : class
            {
                return $"{PIPE_BASE_URL}/{typeof(TService).Name}";
            }
        }
    }
}
