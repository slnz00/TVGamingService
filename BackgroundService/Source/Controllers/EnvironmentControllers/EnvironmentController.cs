using System;
using System.Collections.Generic;
using BackgroundService.Source.Controllers.Models;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using BackgroundService.Source.Services.Jobs.Models;
using Core.Utils;
using static BackgroundService.Source.Services.Jobs.Models.JobOptions;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal abstract class EnvironmentController
    {
        protected ServiceProvider Services { get; private set; }
        protected LoggerProvider Logger { get; private set; }

        public EnvironmentConfig Config { get; private set; }
        public EnvironmentJobsConfig JobsConfig { get; private set; }

        public Environments Environment { get; private set; }
        public MainController MainController { get; private set; }

        public string EnvironmentName => Enum.GetName(typeof(Environments), Environment);

        private List<string> environmentJobIds = new List<string>();

        public EnvironmentController(
            Environments environment,
            EnvironmentConfig config,
            EnvironmentJobsConfig jobConfig,
            MainController mainController,
            ServiceProvider services
        )
        {
            Services = services;
            Environment = environment;
            Config = config;
            JobsConfig = jobConfig;
            Logger = new LoggerProvider(GetType().Name);
            MainController = mainController;
        }

        public void Setup()
        {
            OnSetup();

            CreateEnvironmentJobsForEvent("Setup");
        }

        public void Reset()
        {
            CreateEnvironmentJobsForEvent("Reset", new List<JobTriggerMode> { JobTriggerMode.SYNC });

            OnReset();
        }

        public void Teardown()
        {
            CreateEnvironmentJobsForEvent("Teardown", new List<JobTriggerMode> { JobTriggerMode.SYNC });

            OnTeardown();

            RemoveAllEnvironmentJobs();
        }

        protected abstract void OnSetup();

        protected abstract void OnReset();

        protected abstract void OnTeardown();

        protected void CreateEnvironmentJob(JobOptions options)
        {
            bool jobAdded = Services.Jobs.CreateJob(options);
            if (jobAdded)
            {
                environmentJobIds.Add(options.Id);
            }
        }

        protected void RemoveEnvironmentJob(string id)
        {
            if (!environmentJobIds.Contains(id))
            {
                return;
            }

            Services.Jobs.RemoveJob(id);
            environmentJobIds.Remove(id);
        }

        protected void RemoveAllEnvironmentJobs()
        {
            // Make a copy to avoid issues when removing an element during iteration:
            new List<string>(environmentJobIds).ForEach(RemoveEnvironmentJob);
        }

        private void CreateEnvironmentJobsForEvent(string eventName, List<JobTriggerMode> allowedTriggerModes = null)
        {
            var configs = (List<JobConfig>)JobsConfig.GetType().GetProperty(eventName).GetValue(JobsConfig, null);

            configs.ForEach(config =>
            {
                var options = Services.Jobs.CreateJobOptionsFromJobConfig(config);

                var notSupported = allowedTriggerModes != null && !allowedTriggerModes.Contains(options.TriggerMode);
                if (notSupported)
                {
                    var triggerModeName = EnumUtils.GetName(options.TriggerMode);
                    Logger.Error(
                        $"Failed to create environment job with id: {options.Id}, {triggerModeName} trigger mode is not supported during \"{eventName}\" event."
                    );
                    return;
                }

                CreateEnvironmentJob(options);
            });
        }
    }
}
