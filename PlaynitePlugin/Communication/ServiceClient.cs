using Core;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace PlaynitePlugin.Communication
{
    internal class ServiceClient<TService> : ClientBase<TService> where TService : class
    {
        public TService Service => Channel;

        public ServiceClient() : base
        (
            new ServiceEndpoint
            (
                ContractDescription.GetContract(typeof(TService)),
                new NetNamedPipeBinding(),
                new EndpointAddress(SharedSettings.Playnite.GetServiceAddress<TService>()))
            )
        { }
    }
}
