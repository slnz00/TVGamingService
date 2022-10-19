using BackgroundService.Source.Common;

namespace BackgroundService.Source.Services.Jobs.Models
{
    // Each JobAction implementation must have a JobAction(object options) constructor to work with reflection based factory:
    internal abstract class JobAction : DynamicOptions
    {
        protected JobAction(object options) : base(options) { }

        public abstract void Run(Job job);
    }
}
