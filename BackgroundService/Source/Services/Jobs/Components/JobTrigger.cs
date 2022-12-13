using BackgroundService.Source.Components;
using BackgroundService.Source.Providers;
using Core.Utils;
using System;

namespace BackgroundService.Source.Services.Jobs.Components
{
    // Each JobTrigger implementation must have a JobTrigger(object options) constructor to work with reflection based factory:
    internal abstract class JobTrigger : JobComponent
    {
        public JobTriggerAction Action { get; private set; }
        public bool Closed { get; private set; }
        public bool Opened { get; private set; }

        public JobTrigger(JobTriggerAction action, object options) : base(options)
        {
            Action = action;
        }

        protected abstract void OnOpen();

        protected abstract void OnClose();

        protected void ExecuteTrigger()
        {
            EnsureOwned();

            switch (Action)
            {
                case JobTriggerAction.StartJob:
                    OwnerJob.StartTaskAsync();
                    break;
                case JobTriggerAction.StopJob:
                    OwnerJob.StopTask();
                    break;
                default:
                    throw new InvalidOperationException($"Unimplemented TriggerAction: {EnumUtils.GetName(Action)}");
            }
        }

        public void Open()
        {
            if (Opened)
            {
                Logger.Warn($"JobTrigger ({GetType()}) is already opened");
                return;
            }
            if (Closed)
            {
                throw new InvalidOperationException($"JobTrigger ({GetType()}) is already closed");
            }

            EnsureOwned();

            OnOpen();
            Opened = true;
        }

        public void Close()
        {
            if (Closed)
            {
                Logger.Warn($"JobTrigger ({GetType()}) is already closed");
                return;
            }
            if (!Opened)
            {
                throw new InvalidOperationException($"JobTrigger ({GetType()}) is not in opened state");
            }

            OnClose();
            Closed = true;
        }
    }
}
