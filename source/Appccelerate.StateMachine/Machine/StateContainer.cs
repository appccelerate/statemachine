namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using Infrastructure;
    using States;

    public class StateContainer<TState, TEvent> :
        IExtensionHost<TState, TEvent>,
        IStateMachineInformation<TState, TEvent>,
        ILastActiveStateModifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        public StateContainer(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public List<IExtension<TState, TEvent>> Extensions { get; } = new List<IExtension<TState, TEvent>>();

        public Initializable<TState> InitialStateId { get; } = new Initializable<TState>();

        public TState CurrentState { get; set; }

        public TState CurrentStateId => this.CurrentState;

        public void ForEach(Action<IExtension<TState, TEvent>> action)
        {
            this.Extensions.ForEach(action);
        }

        public IStateDefinition<TState, TEvent> GetLastActiveStateOrNullFor(TState state)
        {
            throw new NotImplementedException();
        }

        public void SetLastActiveStateFor(TState state, IStateDefinition<TState, TEvent> newLastActiveState)
        {
            throw new NotImplementedException();
        }
    }
}
