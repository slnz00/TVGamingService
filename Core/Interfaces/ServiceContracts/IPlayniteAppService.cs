using Core.Models.Playnite;
using System.ServiceModel;

namespace Core.Interfaces.ServiceContracts
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
