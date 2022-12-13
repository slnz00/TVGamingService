using System.Collections.Generic;

namespace BackgroundService.Source.Services.Jobs.Components
{
    internal class JobOptions
    {
        public enum JobMode
        {
            // Runs the job actions synchronously when the job is created, main thread is halted until job execution finishes
            // Trigger option is ignored
            Sync,

            // Runs the job actions asynchronously when the job is created
            // Trigger option is ignored
            Async,

            // Sets up the JobTrigger defined in TriggerWhen option, trigger will start the job asynchronously
            Triggered,
        }

        // Unique job id (job names with "$Internal." are preserved for internal use):
        public string Id { get; set; }

        public JobMode Mode { get; set; }

        // Only used when job's Type set to TRIGGERED, actions are executed after the specified trigger gets fired
        public JobTrigger TriggerWhen { get; set; }

        // Only used when job's Type set to TRIGGERED, actions are executed multiple times, 
        public JobTrigger RepeatUntil { get; set; }

        public List<JobAction> Actions { get; set; }

        // Used in TRIGGERED mode, when RepeatUntil option is defined 
        public int TimeBetweenExecutions { get; set; } = 1000;
    }
}
