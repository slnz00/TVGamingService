using Core.Playnite.Communication.Models;
using System.ServiceModel;

namespace Core.Playnite.Communication.Services
{
    [ServiceContract]
    public interface IPlayniteAppEventsService
    {
        [OperationContract(IsOneWay = true)]
        void SendGameStarting(PlayniteGameInfo gameInfo);

        [OperationContract(IsOneWay = true)]
        void SendGameStopped(PlayniteGameInfo gameInfo);
    }
}
