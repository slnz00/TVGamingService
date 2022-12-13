using Core.Playnite.Communication.Models;
using System.ServiceModel;

namespace Core.Playnite.Communication.Services
{
    [ServiceContract]
    public interface IPlayniteAppEventsService
    {
        [OperationContract]
        void SendGameStarting(PlayniteGameInfo gameInfo);

        [OperationContract(IsOneWay = true)]
        void SendGameStarted(PlayniteGameInfo gameInfo);

        [OperationContract(IsOneWay = true)]
        void SendGameStopped(PlayniteGameInfo gameInfo);
    }
}
