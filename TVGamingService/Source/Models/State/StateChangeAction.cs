using System;

namespace TVGamingService.Source.Models
{
    internal class StateChangeAction<TState>
    {
        public TState FromState { get; set; }
        public TState ToState { get; set; }
        public Action Action { get; set; }

        public StateChangeAction(TState fromState, TState toState, Action action) {
            FromState = fromState;
            ToState = toState;
            Action = action;
        }

        public void Run()
        {
            Action();
        }
    }
}
