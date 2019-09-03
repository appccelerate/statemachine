namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using States;
    using SyntaxNew;

    public class SyntaxStart<TState, TEvent> : ISyntaxStart<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary;
        private readonly IDictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates;

        public SyntaxStart(IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary, IDictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates)
        {
            this.stateDefinitionDictionary = stateDefinitionDictionary;
            this.initiallyLastActiveStates = initiallyLastActiveStates;
        }

        public IEntryActionSyntax<TState, TEvent> In(TState stateId)
        {
            return new StateBuilderNew<TState, TEvent>(stateId, this.stateDefinitionDictionary);
        }

        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return new HierarchyBuilderNew<TState, TEvent>(superStateId, this.stateDefinitionDictionary, this.initiallyLastActiveStates);
        }
    }
}