using BackgroundService.Source.Providers;
using Core.Components;
using System;

namespace BackgroundService.Source.Services.Jobs.Components
{
    internal abstract class JobComponent : DynamicOptions
    {
        protected ServiceProvider Services => Context?.Services;
        protected LoggerProvider Logger => Context?.Logger;
        protected Job OwnerJob => Context?.Job;

        protected Job.Context Context { get; private set; }

        protected JobComponent(object options) : base(options) { }

        public void SetOwner(Job.Context context)
        {
            // Should be only owned by one job and its owner should not change:
            if (OwnerJob != null)
            {
                throw new InvalidOperationException($"{GetType().Name} already belongs to a Job ({OwnerJob.Id})");
            }

            Context = context;
        }

        protected virtual void OnOptionsValidation() { }

        protected void EnsureOwned()
        {
            if (OwnerJob == null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} does not belong to any job, make sure component is owned by a job by calling \"SetOwner\" method"
                );
            }
        }

        protected void ValidateOptions()
        {
            try
            {
                OnOptionsValidation();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to validate options for JobComponent ({GetType().Name}): {ex}");
            }
        }
    }
}
