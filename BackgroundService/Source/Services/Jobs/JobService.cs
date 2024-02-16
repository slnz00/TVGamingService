using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Jobs.Components;
using Core.Configs;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using static BackgroundService.Source.Services.Jobs.Components.JobOptions;

namespace BackgroundService.Source.Services.Jobs
{
    internal class JobService : Service
    {
        private const string JOB_TRIGGER_NAMESPACE = "BackgroundService.Source.Services.Jobs.Components.JobTriggers";
        private const string JOB_ACTION_NAMESPACE = "BackgroundService.Source.Services.Jobs.Components.JobActions";

        private readonly Dictionary<string, Job> jobs = new Dictionary<string, Job>();

        public JobService(ServiceProvider services) : base(services) { }

        public bool CreateJob(JobConfig config)
        {
            var options = CreateJobOptionsFromJobConfig(config);

            return CreateJob(options);
        }

        public bool CreateJob(JobOptions options)
        {
            if (jobs.ContainsKey(options.Id))
            {
                Logger.Error($"Failed to create job, a job with id: {options.Id} already exists");
                return false;
            }

            var job = new Job(Services, options);

            jobs[options.Id] = job;
            job.Open();

            return true;
        }

        public bool RemoveJob(string id)
        {
            if (!jobs.ContainsKey(id))
            {
                return false;
            }

            var job = jobs[id];

            jobs.Remove(id);
            job.Close();

            return true;
        }

        public Job GetJob(string id)
        {
            return jobs.ContainsKey(id) ? jobs[id] : null;
        }

        public JobOptions CreateJobOptionsFromJobConfig(JobConfig jobConfig)
        {
            return new JobOptions()
            {
                Id = jobConfig.Id,
                Mode = EnumUtils.GetValue<JobMode>(jobConfig.Mode),
                TriggerWhen = CreateJobTriggerFromConfig(jobConfig.TriggerWhen, JobTriggerAction.StartJob),
                RepeatUntil = CreateJobTriggerFromConfig(jobConfig.RepeatUntil, JobTriggerAction.StopJob),
                Actions = CreateJobActionsFromConfigs(jobConfig.Actions),
                TimeBetweenExecutions = jobConfig.TimeBetweenExecutions,
            };
        }

        private JobTrigger CreateJobTriggerFromConfig(JobTriggerConfig triggerConfig, JobTriggerAction triggerAction)
        {
            if (triggerConfig == null)
            {
                return null;
            }

            var TriggerType = Type.GetType($"{JOB_TRIGGER_NAMESPACE}.{triggerConfig.Type}");
            return (JobTrigger)Activator.CreateInstance(TriggerType, triggerAction, triggerConfig.Options);
        }

        private List<JobAction> CreateJobActionsFromConfigs(List<JobActionConfig> actionConfigs)
        {
            if (actionConfigs == null)
            {
                return new List<JobAction>();
            }

            JobAction CreateFromConfig(JobActionConfig config)
            {
                var ActionType = Type.GetType($"{JOB_ACTION_NAMESPACE}.{config.Type}");
                return (JobAction)Activator.CreateInstance(ActionType, config.Options);
            }

            return actionConfigs
                .Select(CreateFromConfig)
                .ToList();
        }
    }
}
