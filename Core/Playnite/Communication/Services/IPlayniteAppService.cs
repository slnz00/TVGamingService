using Core.Playnite.Communication.Models;
using Core.Playnite.Communication.Models.Commands;
using System.ServiceModel;

namespace Core.Playnite.Communication.Services
{
    [ServiceContract]
    public interface IPlayniteAppService
    {
        [OperationContract]
        void SendGameStarting(PlayniteGameInfo gameInfo);

        [OperationContract(IsOneWay = true)]
        void SendGameStarted(PlayniteGameInfo gameInfo);

        [OperationContract(IsOneWay = true)]
        void SendGameStopped(PlayniteGameInfo gameInfo);

        [OperationContract]
        AsyncPlayniteTask GetAsyncTask();
    }
}
