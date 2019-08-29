namespace Appccelerate.StateMachine.Machine
{
    using System;
    using SyntaxNew;

    public class SyntaxStart<TState, TEvent> : ISyntaxStart<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary;

        public SyntaxStart(IStateDictionaryNew<TState, TEvent> stateDefinitionDictionary)
        {
            this.stateDefinitionDictionary = stateDefinitionDictionary;
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