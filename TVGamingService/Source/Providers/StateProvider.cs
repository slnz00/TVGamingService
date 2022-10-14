using System;
using TVGamingService.Source.Models;

namespace TVGamingService.Source.Providers
{
    internal class StateProvider
    {
        private State<Environments> environmentState;

        public Environments Environment => environmentState.Value;

        public StateProvider()
        {
            environmentState = new State<Environments>("Environment", Environments.PC);
        }

        public void ChangeEnvironment(Environments newEnvironment)
        {
            environmentState.SetState(newEnvironment);
        }

        public void RegisterEnvironmentChangeAction(Environments fromEnvironment, Environments toEnvironment, Action action)
        {
            var changeAction = new StateChangeAction<Environments>(fromEnvironment, toEnvironment, action);
            environmentState.RegisterChangeAction(changeAction);
        }
    }
}
