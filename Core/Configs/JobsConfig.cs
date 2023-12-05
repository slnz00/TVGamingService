using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Configs
{
    public class JobsConfig
    {
        public static JobsConfig ReadFromFile(string filePath)
        {
            string configJson = File.ReadAllText(filePath, Encoding.Default);
            return JsonConvert.DeserializeObject<JobsConfig>(configJson);
        }

        public static JobsConfig WriteToFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public EnvironmentJobsConfig PC { get; set; } = new EnvironmentJobsConfig();
        public EnvironmentJobsConfig TV { get; set; } = new EnvironmentJobsConfig();
    }

    public class EnvironmentJobsConfig {
        public List<JobConfig> Setup { get; set; } = new List<JobConfig>();
        public List<JobConfig> Reset { get; set; } = new List<JobConfig>();
        public List<JobConfig> Teardown { get; set; } = new List<JobConfig>();
    }

    public class JobConfig
    {
        public string Id { get; set; }

        public string Mode { get; set; }

        public JobTriggerConfig TriggerWhen { get; set; }

        public JobTriggerConfig RepeatUntil { get; set; }

        public List<JobActionConfig> Actions { get; set; }

        public int TimeBetweenExecutions { get; set; } = 500;
    }

    public class JobActionConfig
    {
        public string Type { get; set; }
        public JObject Options { get; set; } = new JObject();
    }

    public class JobTriggerConfig
    {
        public string Type { get; set; }
        public JObject Options { get; set; } = new JObject();
    }
}
