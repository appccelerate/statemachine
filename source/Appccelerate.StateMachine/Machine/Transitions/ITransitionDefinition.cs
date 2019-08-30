namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;
    using System.Collections.Generic;
    using ActionHolders;
    using GuardHolders;
    using States;

    public interface ITransitionDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        IStateDefinition<TState, TEvent> Source { get; }

        IStateDefinition<TState, TEvent> Target { get; }

        IGuardHolder Guard { get; }

        IEnumerable<IActionHolder> Actions { get; }

        bool IsInternalTransition { get; }
    }
}