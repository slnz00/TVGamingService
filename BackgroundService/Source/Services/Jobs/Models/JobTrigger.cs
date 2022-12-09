using BackgroundService.Source.Components;
using BackgroundService.Source.Providers;
using Core.Utils;
using System;

namespace BackgroundService.Source.Services.Jobs.Models
{
    // Each JobTrigger implementation must have a JobTrigger(object options) constructor to work with reflection based factory:
    internal abstract class JobTrigger : DynamicOptions
    {
        public enum TriggerAction
        {
            START_JOB_TASK,
            CLOSE_JOB
        }

        public bool Closed => closed;

        protected ServiceProvider Services => context?.Services;
        protected LoggerProvider Logger => context?.Logger;
        protected Job OwnerJob => context?.Job;

        private Job.Context context = null;
        private bool closed = false;
        private TriggerAction action;

        public JobTrigger(TriggerAction action, object options) : base(options)
        {
            this.action = action;
        }

        protected void ExecuteTrigger()
        {
            if (OwnerJob == null)
            {
                throw new InvalidOperationException("JobTrigger does not have an owner Job.");
            }

            switch (action)
            {
                case TriggerAction.START_JOB_TASK:
                    OwnerJob.StartTaskAsync();
                    break;
                case TriggerAction.CLOSE_JOB:
                    OwnerJob.Close();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown TriggerAction: {EnumUtils.GetName(action)}");
            }
        }

        public void StartListening(Job.Context context)
        {
            if (closed)
            {
                throw new InvalidOperationException("JobTrigger already closed");
            }

            // Should be only owned by one job and its owner should not change:
            if (OwnerJob != null)
            {
                throw new InvalidOperationException("JobTrigger already started (has an owner Job)");
            }

            this.context = context;

            OnSetup();
        }

        public void StopListening()
        {
            if (closed)
            {
                return;
            }

            OnTeardown();

            closed = true;
            context = null;
        }

        // Trigger listeners, resources should be setup during setup event:
        protected abstract void OnSetup();

        // Trigger listeners, resources should be closed and cleaned up during teardown event:
        protected abstract void OnTeardown();
    }
}
