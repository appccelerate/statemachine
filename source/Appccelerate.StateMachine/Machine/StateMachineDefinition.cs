namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using Events;
    using States;
    using Transitions;

    public class StateMachineDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IFactory<TState, TEvent> factory;
        private readonly IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> stateDefinitions;
        private readonly Dictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates;

        public StateMachineDefinition(
            IFactory<TState, TEvent> factory,
            IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> stateDefinitions,
            Dictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates)
        {
            this.factory = factory;
            this.stateDefinitions = stateDefinitions;
            this.initiallyLastActiveStates = initiallyLastActiveStates;
        }

        public PassiveStateMachine<TState, TEvent> CreatePassiveStateMachine()
        {
            var name = typeof(PassiveStateMachine<TState, TEvent>).FullNameToString();
            return this.CreatePassiveStateMachine(name);
        }

        public PassiveStateMachine<TState, TEvent> CreatePassiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            foreach (var stateIdAndLastActiveState in this.initiallyLastActiveStates)
            {
                stateContainer.SetLastActiveStateFor(stateIdAndLastActiveState.Key, stateIdAndLastActiveState.Value);
            }

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer, stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var stateMachine = new StateMachine<TState, TEvent>(this.factory, stateLogic);

            return new PassiveStateMachine<TState, TEvent>(stateMachine, stateContainer, this.stateDefinitions);
        }

        public ActiveStateMachine<TState, TEvent> CreateActiveStateMachine()
        {
            var name = typeof(ActiveStateMachine<TState, TEvent>).FullNameToString();
            return this.CreateActiveStateMachine(name);
        }

        public ActiveStateMachine<TState, TEvent> CreateActiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            foreach (var stateIdAndLastActiveState in this.initiallyLastActiveStates)
            {
                stateContainer.SetLastActiveStateFor(stateIdAndLastActiveState.Key, stateIdAndLastActiveState.Value);
            }

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer, stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var stateMachine = new StateMachine<TState, TEvent>(this.factory, stateLogic);

            return new ActiveStateMachine<TState, TEvent>(stateMachine, stateContainer, this.stateDefinitions);
        }
    }
}