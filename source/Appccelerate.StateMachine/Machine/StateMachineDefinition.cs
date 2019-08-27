namespace Appccelerate.StateMachine.Machine
{
    using System;

    public class StateMachineDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary;

        public StateMachineDefinition(IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary)
        {
            this.stateDefinitionDictionary = stateDefinitionDictionary;
        }
    }
}