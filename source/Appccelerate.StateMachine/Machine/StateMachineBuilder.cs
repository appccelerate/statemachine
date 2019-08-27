namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using SyntaxNew;

    public class StateMachineBuilder<TState, TEvent> : ISyntaxStart<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly List<Func<ISyntaxStart<TState, TEvent>, object>> setupFunctions = new List<Func<ISyntaxStart<TState, TEvent>, object>>();

        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary = new StateDictionaryNew<TState, TEvent>();

        public StateMachineBuilder<TState, TEvent> WithConfiguration(
            Func<ISyntaxStart<TState, TEvent>, object> setupFunction)
        {
            this.setupFunctions.Add(setupFunction);
            return this;
        }

        public IEntryActionSyntax<TState, TEvent> In(TState stateId)
        {
            return new StateBuilderNew<TState, TEvent>(stateId, this.stateDefinitionDictionary);
        }

        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return new HierarchyBuilderNew<TState, TEvent>(superStateId, this.stateDefinitionDictionary);
        }
    }
}