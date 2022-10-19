using System.Collections.Generic;

namespace BackgroundService.Source.Services.Jobs.Models
{
    internal class JobOptions
    {
        public enum JobExecutionMode
        {
            // Runs job actions once
            // Timeout, TimeBetweenExecutions options are ignored
            RUN_ONCE,

            // Runs actions several times until Timeout time elapses
            REPEAT,
        }

        public enum JobTriggerMode
        {
            // Runs the job actions synchronously when the job is created, main thread is halted until job execution finishes
            // Trigger option is ignored
            SYNC,

            // Runs the job actions asynchronously when the job is created
            // Trigger option is ignored
            ASYNC,

            // Sets up the JobTrigger defined in Trigger option, trigger will start the job asynchronously
            ASYNC_TRIGGER,
        }

        // Unique job id (job names with "$Internal." are preserved for internal use):
        public string Id { get; set; }

        public JobExecutionMode ExecutionMode { get; set; }

        public JobTriggerMode TriggerMode { get; set; }

        // Only used when TriggerMode set to ASYNC_TRIGGER
        public JobTrigger Trigger { get; set; }

        public List<JobAction> Actions { get; set; }

        // Used in REPEAT mode, Actions will be ran multiple times until the indicated time amount elapses (milliseconds)
        public int Timeout { get; set; } = 0;

        // Used in REPEAT mode, time amount before executing actions again
        public int TimeBetweenExecutions { get; set; } = 500;
    }
}
