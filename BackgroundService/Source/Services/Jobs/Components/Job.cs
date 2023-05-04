using BackgroundService.Source.Providers;
using Core.Components;
using System;
using System.Threading.Tasks;

using static BackgroundService.Source.Services.Jobs.Components.JobOptions;

namespace BackgroundService.Source.Services.Jobs.Components
{
    internal class Job
    {
        public class Context
        {
            public Job Job { get; set; }
            public ServiceProvider Services { get; set; }
            public LoggerProvider Logger { get; set; }
        }

        public JobOptions Options { get; private set; }

        public string Id => Options.Id;
        public bool Running => jobTask != null && jobTask.IsAlive;
        public bool Closed { get; private set; }

        private readonly object threadLock = new object();

        private readonly Context context;
        private ManagedTask jobTask;

        private ServiceProvider Services => context.Services;
        private LoggerProvider Logger => context.Logger;

        public Job(ServiceProvider services, JobOptions options)
        {
            Options = options;

            context = new Context
            {
                Job = this,
                Services = services,
                Logger = new LoggerProvider($"Job:{options.Id}")
            };

            ValidateOptions();

            SetupActions();
            SetupTriggers();
        }

        public void Open()
        {
            lock (threadLock)
            {
                if (Options.Mode == JobMode.Sync)
                {
                    StartTask();
                }
                else if (Options.Mode == JobMode.Async)
                {
                    StartTaskAsync();
                }
                else if (Options.Mode == JobMode.Triggered)
                {
                    OpenStartTrigger();
                }
            }
        }

        public void Close()
        {
            lock (threadLock)
            {
                if (Closed)
                {
                    return;
                }

                StopTask();
                CloseTriggers();

                Closed = true;
            }
        }

        public void StartTask()
        {
            lock (threadLock)
            {
                StartTask(true);
            }
        }

        public void StartTaskAsync()
        {
            lock (threadLock)
            {
                StartTask(false);
            }
        }

        private void StartTask(bool sync)
        {
            if (Running)
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

            CreateAndRunJobTask();
            OpenStopTrigger();

            if (sync)
            {
                jobTask.Wait();
            }
        }

        public void StopTask()
        {
            lock (threadLock)
            {
                if (Running)
                {
                    jobTask.Cancel();
                }

                jobTask = null;

                ResetJobActions();
                OpenStartTrigger();
            }
        }

        private void ValidateOptions()
        {
            if (Options.Mode != JobMode.Triggered && Options.TriggerWhen != null)
            {
                Logger.Warn("TriggerWhen option is defined, while job type set to TRIGGERED");
            }

            if (Options.Mode == JobMode.Triggered && Options.TriggerWhen == null)
            {
                throw new NullReferenceException("TriggerWhen option is null, while job type set to TRIGGERED");
            }
        }

        private void SetupActions()
        {
            Options.Actions.ForEach(action => action.SetOwner(context));
        }

        private void SetupTriggers()
        {
            Options.TriggerWhen?.SetOwner(context);
            Options.RepeatUntil?.SetOwner(context);
        }

        private void ResetJobActions()
        {
            Options.Actions?.ForEach(action => action.Reset());
        }

        private void OpenStartTrigger()
        {
            Options.TriggerWhen?.Open();
        }

        private void OpenStopTrigger()
        {
            Options.RepeatUntil?.Open();
        }

        private void CloseTriggers()
        {
            Options.TriggerWhen?.Close();
            Options.RepeatUntil?.Close();
        }

        private void CreateAndRunJobTask()
        {
            jobTask = ManagedTask.Run(async (ctx) =>
            {
                Action RunActionsOnce = () =>
                {
                    foreach (var action in Options.Actions)
                    {
                        action.Execute();
                    }
                };

                Func<Task> RunActionsUntilCancellation = async () =>
                {
                    while (!ctx.Cancellation.IsCancellationRequested)
                    {
                        RunActionsOnce();

                        await Task.Delay(Options.TimeBetweenExecutions, ctx.Cancellation.Token);
                    }
                };

                try
                {
                    if (Options.RepeatUntil != null)
                    {
                        await RunActionsUntilCancellation();
                    }
                    else
                    {
                        RunActionsOnce();
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    Logger.Error($"An exception occurred while running job actions: {ex}");
                }
            });
        }
    }
}
