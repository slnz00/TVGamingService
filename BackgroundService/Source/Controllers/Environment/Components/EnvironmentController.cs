﻿using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Jobs.Components;
using Core.Models.Configs;
using Core.Utils;
using System.Collections.Generic;
using static BackgroundService.Source.Services.Jobs.Components.JobOptions;

namespace BackgroundService.Source.Controllers.Environment.Components
{
    internal abstract class EnvironmentController
    {
        protected ServiceProvider Services { get; private set; }
        protected LoggerProvider Logger { get; private set; }

        public EnvironmentJobsConfig JobsConfig => Services.Config.GetJobsConfigForEnvironment(EnvironmentType);

        public MainController MainController { get; private set; }

        public Environments EnvironmentType { get; private set; }
        public string EnvironmentName => EnumUtils.GetName(EnvironmentType);

        private readonly Dictionary<string, bool> jobsCreated = new Dictionary<string, bool>();
        private readonly List<string> environmentJobIds = new List<string>();

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

        public void EnsureSetupJobsAreCreated()
        {
            if (AreJobsCreatedForEvent("Setup"))
            {
                return;
            }

            CreateEnvironmentJobsForEvent("Setup");
        }

        public bool Validate()
        {
            return OnValidate();
        }

        public void Setup()
        {
            OnSetup();

            CreateEnvironmentJobsForEvent("Setup");
        }

        public void Reset()
        {
            OnReset();

            CreateEnvironmentJobsForEvent("Reset", new List<JobMode> { JobMode.Sync });
        }

        public void Teardown()
        {
            OnTeardown();

            CreateEnvironmentJobsForEvent("Teardown", new List<JobMode> { JobMode.Sync });

            RemoveAllEnvironmentJobs();
        }

        protected abstract bool OnValidate();

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
            jobsCreated[eventName] = true;

            var configs = (List<JobConfig>)JobsConfig.GetType().GetField(eventName).GetValue(JobsConfig);

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

        private bool AreJobsCreatedForEvent(string eventName)
        {
            var exists = jobsCreated.TryGetValue(eventName, out var created);

            if (!exists)
            {
                return false;
            }
            return created;
        }
    }
}
