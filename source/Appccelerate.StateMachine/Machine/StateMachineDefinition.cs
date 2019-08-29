namespace Appccelerate.StateMachine.Machine
{
    using System;
    using States;

    public class StateMachineDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitions;

        public StateMachineDefinition(IStateDictionaryNew<TState, TEvent> stateDefinitions)
        {
            this.stateDefinitions = stateDefinitions;
        }

        public PassiveStateMachine<TState, TEvent> CreatePassiveStateMachine()
        {
            return this.CreatePassiveStateMachine(default(string));
        }

        public PassiveStateMachine<TState, TEvent> CreatePassiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            var factory = new StandardFactory<TState, TEvent>();

            var stateLogic = new StateLogic<TState, TEvent>(factory, stateContainer, stateContainer, () => null);

            var stateMachine = new StateMachine<TState, TEvent>(factory, stateLogic, this.stateDefinitions);

            return new PassiveStateMachine<TState, TEvent>(stateMachine, stateContainer);
        }
    }
}