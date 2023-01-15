using BackgroundService.Source.Services.ThirdParty.Playnite.Communication.Services;
using Core.Playnite.Communication;
using Core.Playnite.Communication.Services;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace BackgroundService.Source.Services.ThirdParty.Playnite.Communication
{
    internal class AppCommunicationHost
    {
        private readonly List<ServiceHost> serviceHosts = new List<ServiceHost>();

        public AppCommunicationHost(PlayniteAppService.Events playniteAppEvents)
        {
            CreateServiceHost<IPlayniteAppService>(new PlayniteAppService(playniteAppEvents));
        }

        public void Open()
        {
            serviceHosts.ForEach(serviceHost => serviceHost.Open());
        }

        public void Close()
        {
            serviceHosts.ForEach(serviceHost => serviceHost.Close());
        }

        private ServiceHost CreateServiceHost<TServiceInterface>(TServiceInterface service)
            where TServiceInterface : class
        {
            var pipeUri = new Uri(PlayniteCommunicationSettings.PIPE_BASE_URL);
            var host = new ServiceHost(service, new Uri[] { pipeUri });

            host.AddServiceEndpoint(typeof(TServiceInterface), new NetNamedPipeBinding(), typeof(TServiceInterface).Name);

            serviceHosts.Add(host);
            return host;
        }

    }
}
