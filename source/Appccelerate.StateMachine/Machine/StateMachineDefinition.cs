namespace Appccelerate.StateMachine.Machine
{
    using System;
    using States;
    using Transitions;

    public class StateMachineDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitions;

        public StateMachineDefinition(IStateDictionaryNew<TState, TEvent> stateDefinitions)
        {
            this.stateDefinitions = stateDefinitions;
        }

        public PassiveStateMachine<TState, TEvent> CreatePassiveStateMachine()
        {
            return this.CreatePassiveStateMachine(default(string));
        }

        public PassiveStateMachine<TState, TEvent> CreatePassiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            var factory = new StandardFactory<TState, TEvent>();

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer, stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var stateMachine = new StateMachine<TState, TEvent>(factory, stateLogic, this.stateDefinitions);

            return new PassiveStateMachine<TState, TEvent>(stateMachine, stateContainer);
        }

        public ActiveStateMachine<TState, TEvent> CreateActiveStateMachine()
        {
            return this.CreateActiveStateMachine(default(string));
        }

        public ActiveStateMachine<TState, TEvent> CreateActiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            var factory = new StandardFactory<TState, TEvent>();

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer, stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var stateMachine = new StateMachine<TState, TEvent>(factory, stateLogic, this.stateDefinitions);

            return new ActiveStateMachine<TState, TEvent>(stateMachine, stateContainer);
        }

        // todo: wtjerry
        public StateMachine<TState, TEvent> CreateStateMachine(StateContainer<TState, TEvent> stateContainer)
        {
            var factory = new StandardFactory<TState, TEvent>();

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer, stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            return new StateMachine<TState, TEvent>(factory, stateLogic, this.stateDefinitions);
        }
    }
}