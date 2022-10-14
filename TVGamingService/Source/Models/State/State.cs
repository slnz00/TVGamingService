using System;
using System.Collections.Generic;
using TVGamingService.Source.Providers;

namespace TVGamingService.Source.Models
{
    internal class State<TState>
    {
        private LoggerProvider Logger;

        private string name;
        private TState value;
        private List<StateChangeAction<TState>> changeActions = new List<StateChangeAction<TState>>();

        public string Name => name;
        public TState Value => value;

        public State(string name, TState defaultValue)
        {
            Logger = new LoggerProvider($"State:{name}");

            this.name = name;
            value = defaultValue;
        }

        public void RegisterChangeAction(StateChangeAction<TState> changeAction)
        {
            changeActions.Add(changeAction);
        }

        public void SetState(TState newValue)
        {
            TState oldValue = value;

            value = newValue;
            Logger.Debug($"State changed: {oldValue} -> {newValue}");

            var changeActions = GetChangeActions(oldValue, newValue);
            changeActions.ForEach(action => action.Run());
            Logger.Debug($"State change actions successfully executed, count: {changeActions.Count}");
        }

        private List<StateChangeAction<TState>> GetChangeActions(TState oldValue, TState newValue)
        {
            var actions = changeActions.FindAll(a => a.FromState.Equals(oldValue) && a.ToState.Equals(newValue));

            if (actions.Count == 0)
            {
                Logger.Warn($"No registered actions were found for state change: {oldValue} -> {newValue}");
            }

            return actions;
        }
    }
}
