namespace Appccelerate.StateMachine.SyntaxNew
{
    using System;

    public interface ISyntaxStart<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        IEntryActionSyntax<TState, TEvent> In(TState state);

        IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId);
    }
}