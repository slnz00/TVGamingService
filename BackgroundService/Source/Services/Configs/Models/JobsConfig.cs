using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BackgroundService.Source.Services.Configs.Models
{
    internal class JobsConfig
    {
        public EnvironmentJobsConfig PC { get; set; } = new EnvironmentJobsConfig();
        public EnvironmentJobsConfig TV { get; set; } = new EnvironmentJobsConfig();
    }

    internal class EnvironmentJobsConfig {
        public List<JobConfig> Setup { get; set; } = new List<JobConfig>();
        public List<JobConfig> Reset { get; set; } = new List<JobConfig>();
        public List<JobConfig> Teardown { get; set; } = new List<JobConfig>();
    }

    internal class JobConfig
    {
        public string Id { get; set; }

        public string Mode { get; set; }

        public JobTriggerConfig TriggerWhen { get; set; }

        public JobTriggerConfig RepeatUntil { get; set; }

        public List<JobActionConfig> Actions { get; set; }

        public int TimeBetweenExecutions { get; set; } = 500;
    }

    internal class JobActionConfig
    {
        public string Type { get; set; }
        public JObject Options { get; set; } = new JObject();
    }

    internal class JobTriggerConfig
    {
        public string Type { get; set; }
        public JObject Options { get; set; } = new JObject();
    }
}
