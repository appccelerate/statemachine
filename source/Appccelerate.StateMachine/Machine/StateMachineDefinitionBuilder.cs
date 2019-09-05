namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using Events;
    using States;
    using Syntax;

    public class StateMachineDefinitionBuilder<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly List<Func<ISyntaxStart<TState, TEvent>, object>> setupFunctions = new List<Func<ISyntaxStart<TState, TEvent>, object>>();
        private IFactory<TState, TEvent> factory = new StandardFactory<TState, TEvent>();

        public StateMachineDefinitionBuilder<TState, TEvent> WithCustomFactory(IFactory<TState, TEvent> customFactory)
        {
            this.factory = customFactory;
            return this;
        }

        public StateMachineDefinitionBuilder<TState, TEvent> WithConfiguration(
            Func<ISyntaxStart<TState, TEvent>, object> setupFunction)
        {
            this.setupFunctions.Add(setupFunction);
            return this;
        }

        public StateMachineDefinition<TState, TEvent> Build()
        {
            var stateDefinitionDictionary = new StateDictionary<TState, TEvent>();
            var initiallyLastActiveStates = new Dictionary<TState, IStateDefinition<TState, TEvent>>();
            var syntaxStart = new SyntaxStart<TState, TEvent>(this.factory, stateDefinitionDictionary, initiallyLastActiveStates);

            this.setupFunctions.ForEach(f => f(syntaxStart));

            return new StateMachineDefinition<TState, TEvent>(this.factory, stateDefinitionDictionary.ReadOnlyDictionary, initiallyLastActiveStates);
        }
    }
}