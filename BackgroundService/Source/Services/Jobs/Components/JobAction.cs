using BackgroundService.Source.Components;
using System;

namespace BackgroundService.Source.Services.Jobs.Components
{
    internal abstract class JobAction : JobComponent
    {
        protected JobAction(object options) : base(options) { }

        public void Execute()
        {
            EnsureOwned();

            ValidateOptions();

            OnExecution();
        }

        protected abstract void OnExecution();
    }
}
