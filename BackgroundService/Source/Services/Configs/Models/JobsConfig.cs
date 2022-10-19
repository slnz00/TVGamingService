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

        public string ExecutionMode { get; set; }

        public string TriggerMode { get; set; }

        // Only used when TriggerMode set to ASYNC_TRIGGER
        public JobTriggerConfig Trigger { get; set; }

        public List<JobActionConfig> Actions { get; set; }

        // Used in REPEAT mode, Actions will be ran multiple times until the indicated time amount elapses (milliseconds)
        public int Timeout { get; set; } = 0;

        // Used in REPEAT mode, time amount before executing actions again
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
