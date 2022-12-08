namespace Core.Playnite.Communication
{
    public static class PlayniteCommunicationSettings
    {
        public static readonly string PIPE_GUID = "4dbc9aba-f0c2-4ba0-a637-5ca12f3a621a";
        public static readonly string PIPE_BASE_URL = $"net.pipe://localhost/TVGamingService-Playnite-{PIPE_GUID}";

        public static string GetServiceAddress<TServiceInterface>()
            where TServiceInterface : class
        {
            return $"{PIPE_BASE_URL}/{typeof(TServiceInterface).Name}";
        }
    }
}