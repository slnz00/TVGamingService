using BackgroundService.Source.Providers;
using Core.Components;
using System;
using System.Threading.Tasks;

using static BackgroundService.Source.Services.Jobs.Models.JobOptions;

namespace BackgroundService.Source.Services.Jobs.Models
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
        public bool IsRunning => jobTask != null ? jobTask.IsAlive : false;
        public bool Closed => closed;

        private readonly object threadLock = new object();

        private readonly Context context;
        private ServiceProvider Services => context.Services;
        private LoggerProvider Logger => context.Logger;

        private ManagedTask jobTask;

        private bool closed = false;

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
        }

        public void Open()
        {
            lock (threadLock)
            {
                if (Options.Mode == JobMode.SYNC)
                {
                    StartTask();
                }
                else if (Options.Mode == JobMode.ASYNC)
                {
                    StartTaskAsync();
                }
                else if (Options.Mode == JobMode.TRIGGERED)
                {
                    StartTriggers();
                }
            }
        }

        public void Close()
        {
            lock (threadLock)
            {
                if (closed)
                {
                    return;
                }

                StopTask();
                StopTriggers();

                closed = true;
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

            CreateAndRunJobTask();

            if (sync)
            {
                jobTask.Wait();
            }
        }

        public void StopTask()
        {
            lock (threadLock)
            {
                if (IsRunning)
                {
                    jobTask.Cancel();
                    jobTask.Wait();
                }

                jobTask = null;
            }
        }

        private void ValidateOptions()
        {
            if (Options.Mode != JobMode.TRIGGERED && Options.TriggerWhen != null)
            {
                Logger.Warn("TriggerWhen option is defined, while job type set to TRIGGERED");
            }

            if (Options.Mode == JobMode.TRIGGERED && Options.TriggerWhen == null)
            {
                throw new NullReferenceException("TriggerWhen option is null, while job type set to TRIGGERED");
            }
        }

        private void StartTriggers() {
            Options.TriggerWhen.StartListening(context);
            Options.RepeatUntil?.StartListening(context);
        }

        private void StopTriggers()
        {
            Options.TriggerWhen?.StopListening();
            Options.RepeatUntil?.StopListening();
        }

        private void CreateAndRunJobTask()
        {
            jobTask = ManagedTask.Run(async (ctx) =>
            {
                Action RunActionsOnce = () =>
                {
                    foreach (var action in Options.Actions)
                    {
                        if (ctx.Cancellation.IsCancellationRequested)
                        {
                            break;
                        }

                        action.Run(context);
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
