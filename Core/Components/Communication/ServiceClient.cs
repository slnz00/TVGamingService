using System.ServiceModel;
using System.ServiceModel.Description;

namespace Core.Components.Communication
{
    public class ServiceClient<TService> : ClientBase<TService> where TService : class
    {
        public TService Service => Channel;

        public ServiceClient() : base
        (
            new ServiceEndpoint
            (
                ContractDescription.GetContract(typeof(TService)),
                new NetNamedPipeBinding(),
                new EndpointAddress(SharedSettings.Communication.GetServiceURL<TService>()))
            )
        { }
    }
}
