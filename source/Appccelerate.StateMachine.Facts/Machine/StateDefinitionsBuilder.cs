namespace Appccelerate.StateMachine.Facts.Machine
{
    using System;
    using System.Collections.Generic;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using StateMachine.Syntax;

    public class StateDefinitionsBuilder<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly List<Func<ISyntaxStart<TState, TEvent>, object>> setupFunctions = new List<Func<ISyntaxStart<TState, TEvent>, object>>();

        public StateDefinitionsBuilder<TState, TEvent> WithConfiguration(
            Func<ISyntaxStart<TState, TEvent>, object> setupFunction)
        {
            this.setupFunctions.Add(setupFunction);
            return this;
        }

        public IReadOnlyDictionary<TState, StateDefinition<TState, TEvent>> Build()
        {
            var stateDefinitionDictionary = new StateDictionary<TState, TEvent>();
            var initiallyLastActiveStates = new Dictionary<TState, IStateDefinition<TState, TEvent>>();
            var syntaxStart = new SyntaxStart<TState, TEvent>(stateDefinitionDictionary, initiallyLastActiveStates);

            this.setupFunctions.ForEach(f => f(syntaxStart));

            return stateDefinitionDictionary.ReadOnlyDictionary;
        }
    }
}