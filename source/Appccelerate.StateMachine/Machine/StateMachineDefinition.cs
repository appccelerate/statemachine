namespace Appccelerate.StateMachine.Machine
{
    using System;

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
            var factory = new StandardFactory<TState, TEvent>();
            var stateMachine = new StateMachine<TState, TEvent>(factory);
            return new PassiveStateMachine<TState, TEvent>(name, stateMachine);
        }
    }
}