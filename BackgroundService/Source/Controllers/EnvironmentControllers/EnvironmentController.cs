using System;
using System.Collections.Generic;
using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using BackgroundService.Source.Services.Jobs.Components;
using Core.Utils;

using static BackgroundService.Source.Services.Jobs.Components.JobOptions;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal abstract class EnvironmentController
    {
        protected ServiceProvider Services { get; private set; }
        protected LoggerProvider Logger { get; private set; }

        public EnvironmentConfig Config => Services.Config.GetConfigForEnvironment(EnvironmentType);
        public EnvironmentJobsConfig JobsConfig => Services.Config.GetJobsConfigForEnvironment(EnvironmentType);

        public MainController MainController { get; private set; }

        public Environments EnvironmentType { get; private set; }
        public string EnvironmentName => Enum.GetName(typeof(Environments), EnvironmentType);

        private List<string> environmentJobIds = new List<string>();

        public EnvironmentController(
            Environments environment,
            MainController mainController,
            ServiceProvider services
        )
        {
            Services = services;
            EnvironmentType = environment;
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
            CreateEnvironmentJobsForEvent("Reset", new List<JobMode> { JobMode.Sync });

            OnReset();
        }

        public void Teardown()
        {
            CreateEnvironmentJobsForEvent("Teardown", new List<JobMode> { JobMode.Sync });

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
            CollectionUtils.CloneList(environmentJobIds).ForEach(RemoveEnvironmentJob);
        }

        private void CreateEnvironmentJobsForEvent(string eventName, List<JobMode> allowedJobModes = null)
        {
            var configs = (List<JobConfig>)JobsConfig.GetType().GetProperty(eventName).GetValue(JobsConfig, null);

            configs.ForEach(config =>
            {
                var options = Services.Jobs.CreateJobOptionsFromJobConfig(config);

                var notSupported = allowedJobModes != null && !allowedJobModes.Contains(options.Mode);
                if (notSupported)
                {
                    var triggerModeName = EnumUtils.GetName(options.Mode);
                    Logger.Error(
                        $"Failed to create environment job with id: {options.Id}, {triggerModeName} job mode is not supported during \"{eventName}\" event."
                    );
                    return;
                }

                CreateEnvironmentJob(options);
            });
        }
    }
}
