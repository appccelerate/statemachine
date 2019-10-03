//-------------------------------------------------------------------------------
// <copyright file="StateMachine.cs" company="Appccelerate">
//   Copyright (c) 2008-2019 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.AsyncMachine
{
    using System;
    using System.Threading.Tasks;
    using Events;
    using States;

    /// <summary>
    /// Base implementation of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachine<TState, TEvent> :
        INotifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IFactory<TState, TEvent> factory;
        private readonly IStateLogic<TState, TEvent> stateLogic;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="factory">The factory used to create internal instances.</param>
        /// <param name="stateLogic">The state logic used to handle state changes.</param>
        public StateMachine(IFactory<TState, TEvent> factory, IStateLogic<TState, TEvent> stateLogic)
        {
            this.factory = factory;
            this.stateLogic = stateLogic;
        }

        /// <summary>
        /// Occurs when no transition could be executed.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined;

        /// <summary>
        /// Occurs when an exception was thrown inside a transition of the state machine.
        /// </summary>
        public event EventHandler<TransitionExceptionEventArgs<TState, TEvent>> TransitionExceptionThrown;

        /// <summary>
        /// Occurs when a transition begins.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionBegin;

        /// <summary>
        /// Occurs when a transition completed.
        /// </summary>
        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted;

        private static async Task SwitchStateTo(
            IStateDefinition<TState, TEvent> newState,
            StateContainer<TState, TEvent> stateContainer,
            IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            var oldState = stateContainer.CurrentState;

            stateContainer.CurrentState = newState;

            await stateContainer
                .ForEach(extension =>
                    extension.SwitchedState(stateMachineInformation, oldState, newState))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the state machine by setting the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state of the state machine.</param>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        /// <param name="stateMachineInformation">The state machine information.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Initialize(TState initialState, StateContainer<TState, TEvent> stateContainer, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            await stateContainer.ForEach(extension => extension.InitializingStateMachine(stateMachineInformation, ref initialState))
                .ConfigureAwait(false);

            Initialize(initialState, stateContainer);

            await stateContainer.ForEach(extension => extension.InitializedStateMachine(stateMachineInformation, initialState))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Enters the initial state that was previously set with <see cref="Initialize(TState, StateContainer{TState,TEvent}, IStateMachineInformation{TState,TEvent})"/>.
        /// </summary>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        /// <param name="stateMachineInformation">The state machine information.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EnterInitialState(
            StateContainer<TState, TEvent> stateContainer,
            IStateMachineInformation<TState, TEvent> stateMachineInformation,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            CheckThatStateMachineIsInitialized(stateContainer);

            await stateContainer.ForEach(extension => extension.EnteringInitialState(stateMachineInformation, stateContainer.InitialStateId.Value))
                .ConfigureAwait(false);

            var context = this.factory.CreateTransitionContext(null, new Missable<TEvent>(), Missing.Value, this);
            await this.EnterInitialState(context, stateContainer, stateMachineInformation, stateDefinitions)
                .ConfigureAwait(false);

            await stateContainer.ForEach(extension => extension.EnteredInitialState(stateMachineInformation, stateContainer.InitialStateId.Value, context))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        /// <param name="stateMachineInformation">The state machine information.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Fire(
            TEvent eventId,
            object eventArgument,
            StateContainer<TState, TEvent> stateContainer,
            IStateMachineInformation<TState, TEvent> stateMachineInformation,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            CheckThatStateMachineIsInitialized(stateContainer);
            CheckThatStateMachineHasEnteredInitialState(stateContainer);

            await stateContainer.ForEach(extension => extension.FiringEvent(stateMachineInformation, ref eventId, ref eventArgument))
                .ConfigureAwait(false);

            var context = this.factory.CreateTransitionContext(stateContainer.CurrentState, new Missable<TEvent>(eventId), eventArgument, this);
            var result = await this.stateLogic.Fire(stateContainer.CurrentState, context, stateContainer)
                .ConfigureAwait(false);

            if (!result.Fired)
            {
                this.OnTransitionDeclined(context);
                return;
            }

            var newState = stateDefinitions[result.NewState];
            await SwitchStateTo(newState, stateContainer, stateMachineInformation)
                .ConfigureAwait(false);

            await stateContainer.ForEach(extension => extension.FiredEvent(stateMachineInformation, context))
                .ConfigureAwait(false);

            this.OnTransitionCompleted(context, stateMachineInformation);
        }

        public void OnExceptionThrown(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            RethrowExceptionIfNoHandlerRegistered(exception, this.TransitionExceptionThrown);

            this.RaiseEvent(this.TransitionExceptionThrown, new TransitionExceptionEventArgs<TState, TEvent>(context, exception), context, false);
        }

        /// <summary>
        /// Fires the <see cref="TransitionBegin"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        public void OnTransitionBegin(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionBegin, new TransitionEventArgs<TState, TEvent>(transitionContext), transitionContext, true);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void RethrowExceptionIfNoHandlerRegistered<T>(Exception exception, EventHandler<T> exceptionHandler)
            where T : EventArgs
        {
            if (exceptionHandler == null)
            {
                throw new StateMachineException("No exception listener is registered. Exception: ", exception);
            }
        }

        /// <summary>
        /// Fires the <see cref="TransitionDeclined"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition event context.</param>
        private void OnTransitionDeclined(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionDeclined, new TransitionEventArgs<TState, TEvent>(transitionContext), transitionContext, true);
        }

        /// <summary>
        /// Fires the <see cref="TransitionCompleted"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition event context.</param>
        /// <param name="stateMachineInformation">The state machine information.</param>
        private void OnTransitionCompleted(ITransitionContext<TState, TEvent> transitionContext, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            this.RaiseEvent(
                this.TransitionCompleted,
                new TransitionCompletedEventArgs<TState, TEvent>(
                    stateMachineInformation.CurrentStateId,
                    transitionContext),
                transitionContext,
                true);
        }

        /// <summary>
        /// Initializes the state machine by setting the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        private static void Initialize(TState initialState, StateContainer<TState, TEvent> stateContainer)
        {
            if (stateContainer.InitialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }

            stateContainer.InitialStateId.Value = initialState;
        }

        private async Task EnterInitialState(
            ITransitionContext<TState, TEvent> context,
            StateContainer<TState, TEvent> stateContainer,
            IStateMachineInformation<TState, TEvent> stateMachineInformation,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            var initialState = stateDefinitions[stateContainer.InitialStateId.Value];
            var initializer = this.factory.CreateStateMachineInitializer(initialState, context);
            var newStateId = await initializer.EnterInitialState(this.stateLogic, stateContainer).
                ConfigureAwait(false);
            var newStateDefinition = stateDefinitions[newStateId];
            await SwitchStateTo(newStateDefinition, stateContainer, stateMachineInformation)
                .ConfigureAwait(false);
        }

        private void RaiseEvent<T>(EventHandler<T> eventHandler, T arguments, ITransitionContext<TState, TEvent> context, bool raiseEventOnException)
            where T : EventArgs
        {
            try
            {
                if (eventHandler == null)
                {
                    return;
                }

                eventHandler(this, arguments);
            }
            catch (Exception e)
            {
                if (!raiseEventOnException)
                {
                    throw;
                }

                ((INotifier<TState, TEvent>)this).OnExceptionThrown(context, e);
            }
        }

        private static void CheckThatStateMachineIsInitialized(StateContainer<TState, TEvent> stateContainer)
        {
            if (stateContainer.CurrentState == null && !stateContainer.InitialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineNotInitialized);
            }
        }

        private static void CheckThatStateMachineHasEnteredInitialState(StateContainer<TState, TEvent> stateContainer)
        {
            if (stateContainer.CurrentState == null)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineHasNotYetEnteredInitialState);
            }
        }
    }
}