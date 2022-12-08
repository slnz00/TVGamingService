using BackgroundService.Source.Components;
using BackgroundService.Source.Providers;
using System;

namespace BackgroundService.Source.Services.Jobs.Models
{
    // Each JobTrigger implementation must have a JobTrigger(object options) constructor to work with reflection based factory:
    internal abstract class JobTrigger : DynamicOptions
    {
        public bool Closed => closed;

        private ServiceProvider Services => ownerJob.Services;

        private Job ownerJob = null;
        private bool closed = false;

        public JobTrigger(object options) : base(options) { }

        protected void TriggerJob()
        {
            if (ownerJob == null)
            {
                throw new InvalidOperationException("JobTrigger does not have an owner Job.");
            }

            ownerJob.StartAsync();
        }

        public void StartListening(Job ownerJob)
        {
            if (closed)
            {
                throw new InvalidOperationException("JobTrigger already closed");
            }

            // Should be only owned by one job and its owner should not change:
            if (ownerJob != null)
            {
                throw new InvalidOperationException("JobTrigger already started (has an owner Job)");
            }

            this.ownerJob = ownerJob;

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
            ownerJob = null;
        }

        // Trigger listeners, resources should be setup during setup event:
        protected abstract void OnSetup();

        // Trigger listeners, resources should be closed and cleaned up during teardown event:
        protected abstract void OnTeardown();
    }
}
