using Core.Playnite.Communication;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace PlaynitePlugin.Communication
{
    internal class ServiceClient<TServiceInterface> : ClientBase<TServiceInterface> where TServiceInterface : class
    {
        public TServiceInterface Service => Channel;

        public ServiceClient() : base
        (
            new ServiceEndpoint
            (
                ContractDescription.GetContract(typeof(TServiceInterface)),
                new NetNamedPipeBinding(),
                new EndpointAddress(PlayniteCommunicationSettings.GetServiceAddress<TServiceInterface>()))
            )
        { }
    }
}
