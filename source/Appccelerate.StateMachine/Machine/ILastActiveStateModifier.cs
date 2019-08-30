namespace Appccelerate.StateMachine.Machine
{
    using System;
    using States;

    public interface ILastActiveStateModifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        IStateDefinition<TState, TEvent> GetLastActiveStateOrNullFor(TState state);

        void SetLastActiveStateFor(TState state, IStateDefinition<TState, TEvent> newLastActiveState);
    }
}