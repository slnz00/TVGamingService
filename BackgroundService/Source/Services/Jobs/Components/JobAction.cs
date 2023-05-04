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

        public void Reset()
        {
            OnReset();
        }

        protected abstract void OnExecution();
        protected abstract void OnReset();
    }
}
