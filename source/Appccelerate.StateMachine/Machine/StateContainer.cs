namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Infrastructure;

    public class StateContainer<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        public StateContainer(IFactory<TState, TEvent> factory)
        {
            this.States = new StateDictionary<TState, TEvent>(factory);
        }

        public List<IExtension<TState, TEvent>> Extensions { get; } = new List<IExtension<TState, TEvent>>();

        public Initializable<TState> InitialStateId { get; } = new Initializable<TState>();

        public IState<TState, TEvent> CurrentState { get; set; }

        public IStateDictionary<TState, TEvent> States { get; }
    }
}
