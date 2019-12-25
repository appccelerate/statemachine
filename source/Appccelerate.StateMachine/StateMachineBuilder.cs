namespace Appccelerate.StateMachine
{
    using System;

    public static class StateMachineBuilder
    {
        public static AsyncMachine.Building.StateMachineDefinitionBuilder<TState, TEvent> ForAsyncMachine<TState, TEvent>()
            where TState : IComparable
            where TEvent : IComparable
        {
            return new AsyncMachine.Building.StateMachineDefinitionBuilder<TState, TEvent>();
        }

        public static Machine.Building.StateMachineDefinitionBuilder<TState, TEvent> ForMachine<TState, TEvent>()
            where TState : IComparable
            where TEvent : IComparable
        {
            return new Machine.Building.StateMachineDefinitionBuilder<TState, TEvent>();
        }
    }
}