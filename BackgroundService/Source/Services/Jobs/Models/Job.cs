using BackgroundService.Source.Providers;
using Core.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

using static BackgroundService.Source.Services.Jobs.Models.JobOptions;

namespace BackgroundService.Source.Services.Jobs.Models
{
    internal class Job
    {
        public JobOptions Options { get; private set; }
        public string Id => Options.Id;
        public bool IsRunning => AsyncUtils.IsTaskAlive(jobTask);
        public bool Closed => closed;

        private readonly LoggerProvider Logger;
        public readonly ServiceProvider Services;

        private readonly object threadLock = new object();

        private Task jobTask;
        private CancellationTokenSource jobCancellation;

        private bool closed = false;

        public Job(ServiceProvider services, JobOptions options)
        {
            Options = options;
            Services = services;

            Logger = new LoggerProvider($"Job:{options.Id}");

            ValidateOptions();
        }

        public void Execute()
        {
            if (Options.TriggerMode == JobTriggerMode.SYNC)
            {
                StartSync();
            }
            else if (Options.TriggerMode == JobTriggerMode.ASYNC)
            {
                StartAsync();
            }
            else if (Options.TriggerMode == JobTriggerMode.ASYNC_TRIGGER)
            {
                Options.Trigger.StartListening(this);
            }
        }

        public void StartSync()
        {
            lock (threadLock)
            {
                Start(true);
            }
        }

        public void StartAsync()
        {
            lock (threadLock)
            {
                Start(false);
            }
        }

        // After closing a job, it cannot be restarted and its trigger will be destroyed:
        public void Stop(bool close = false)
        {
            lock (threadLock)
            {
                if (IsRunning)
                {
                    jobCancellation.Cancel();
                    jobTask.Wait();
                }

                if (close)
                {
                    Close();
                }

                jobTask = null;
                jobCancellation = null;
            }
        }

        private void Start(bool sync)
        {
            if (IsRunning)
            {
                return;
            }
            if (Closed)
            {
                Logger.Error(
                    "Job start is triggered while job is in closed state, skipping... This error could indicate that the job's trigger is not properly cleaned up during teardown."
                );
                return;
            }

            jobCancellation = new CancellationTokenSource();

            jobTask = Task.Run(() =>
            {
                try
                {
                    RunActions();
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    Logger.Error($"An exception occurred while running job actions: {ex}");
                }
            });

            if (sync)
            {
                jobTask.Wait();
            }
        }

        private void ValidateOptions()
        {
            if (Options.TriggerMode != JobTriggerMode.ASYNC_TRIGGER && Options.Trigger != null)
            {
                Logger.Warn("Trigger option is defined while job is not in ASYNC_TRIGGER mode");
            }

            if (Options.TriggerMode == JobTriggerMode.ASYNC_TRIGGER && Options.Trigger == null)
            {
                throw new NullReferenceException("Trigger option null, while job is in ASYNC_TRIGGER mode");
            }
        }

        private void Close()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Running jobs cannot be closed");
            }
            if (closed)
            {
                return;
            }

            if (Options.TriggerMode == JobTriggerMode.ASYNC_TRIGGER)
            {
                Options.Trigger.StopListening();
            }

            closed = true;
        }

        private void RunActions()
        {
            switch (Options.ExecutionMode)
            {
                case JobExecutionMode.RUN_ONCE:
                    RunActionsOnce();
                    break;
                case JobExecutionMode.REPEAT:
                    RunActionsOnRepeat();
                    break;
                default:
                    throw new NotImplementedException("Unimplemented job execution mode");
            }
        }

        private void RunActionsOnce()
        {
            Options.Actions.ForEach(action =>
            {
                Logger.Debug($"Running job action: {action.GetType().Name}");

                action.Run(this);
            });
        }

        private void RunActionsOnRepeat()
        {
            Func<int> now = () => DateTime.Now.Millisecond;

            var runUntil = now() + Options.Timeout;

            do
            {
                RunActionsOnce();
                Task.Delay(Options.TimeBetweenExecutions, jobCancellation.Token);
            } while (now() < runUntil);
        }
    }
}
