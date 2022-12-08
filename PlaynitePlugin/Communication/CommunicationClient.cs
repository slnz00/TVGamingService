using Core.Playnite.Communication.Services;

namespace PlaynitePlugin.Communication
{
    internal class CommunicationClient
    {
        public readonly ServiceClient<IPlayniteAppEventsService> PlayniteEvents = new ServiceClient<IPlayniteAppEventsService>();
    }
}
