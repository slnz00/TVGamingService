using Core.Playnite.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

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
