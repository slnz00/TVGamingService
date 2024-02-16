using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            return JsonConvert.DeserializeObject<JobsConfig>(configJson, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });
        }

        public static void WriteToFile(JobsConfig config, string filePath)
        {
            var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(filePath, configJson, Encoding.Default);
        }

        public EnvironmentJobsConfig PCEnvironment = new EnvironmentJobsConfig();
        public EnvironmentJobsConfig GameEnvironment = new EnvironmentJobsConfig();
    }

    public class EnvironmentJobsConfig
    {
        public List<JobConfig> Setup = new List<JobConfig>();
        public List<JobConfig> Reset = new List<JobConfig>();
        public List<JobConfig> Teardown = new List<JobConfig>();
    }

    public class JobConfig
    {
        public string Id;

        public string Mode;

        public JobTriggerConfig TriggerWhen;

        public JobTriggerConfig RepeatUntil;

        public List<JobActionConfig> Actions;

        public int TimeBetweenExecutions = 500;
    }

    public class JobActionConfig
    {
        public string Type;
        public JObject Options = new JObject();
    }

    public class JobTriggerConfig
    {
        public string Type;
        public JObject Options = new JObject();
    }
}
