using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Communication.ServiceHosts;
using Core;
using Core.Components.Communication;
using Core.Interfaces.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace BackgroundService.Source.Services.Communication
{
    internal class CommunicationService : Service
    {
        public PlayniteAppService Playnite { get; private set; }

        private readonly List<ServiceHost> serviceHosts = new List<ServiceHost>();

        public CommunicationService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            Playnite = RegisterServiceHost<IPlayniteAppService, PlayniteAppService>(new PlayniteAppService());
        }

        protected override void OnDispose()
        {
            CloseServiceHosts();
        }

        private void CloseServiceHosts()
        {
            serviceHosts.ForEach(serviceHost => serviceHost.Close());
        }

        private TService RegisterServiceHost<TContractInterface, TService>(TService service)
            where TContractInterface : class
            where TService : TContractInterface
        {
            ServiceContractAttribute contractAttribute = (ServiceContractAttribute)Attribute.GetCustomAttribute(
                typeof(TContractInterface),
                typeof(ServiceContractAttribute)
            );

            if (contractAttribute == null)
            {
                Logger.Error($"Failed to register service host, {typeof(TContractInterface).Name} is not a ServiceContract");
            }

            var baseAddress = new Uri(SharedSettings.Communication.GetServiceAddress<TContractInterface>());
            var hostExists = serviceHosts.Any((currentHost) => 
                Uri.Compare(
                    currentHost.BaseAddresses[0],
                    baseAddress,
                    UriComponents.Host | UriComponents.PathAndQuery,
                    UriFormat.SafeUnescaped,
                    StringComparison.OrdinalIgnoreCase
                ) == 0
            );

            if (hostExists) {
                throw new ArgumentException($"A host already exists for the provided service contract: {typeof(TContractInterface)}");
            }

            var host = new ServiceHost(service, new Uri[] { baseAddress });

            host.AddServiceEndpoint(typeof(TContractInterface), new NetNamedPipeBinding(), SharedSettings.Communication.SERVICE_ENDPOINT);
            host.Open();

            serviceHosts.Add(host);

            return service;
        }

    }
}
