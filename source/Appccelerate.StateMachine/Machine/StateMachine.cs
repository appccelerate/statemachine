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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using Events;
    using Infrastructure;
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

        private static void SwitchStateTo(
            IStateDefinition<TState, TEvent> newState,
            StateContainer<TState, TEvent> stateContainer,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            var oldState = stateContainer
                .CurrentStateId
                .Map(x => stateDefinitions[x])
                .ExtractOr(null);

            stateContainer.CurrentStateId = Initializable<TState>.Initialized(newState.Id);

            stateContainer.ForEach(extension =>
                extension.SwitchedState(oldState, newState));
        }

        /// <summary>
        /// Enters the initial state as specified with <paramref name="initialState"/>.
        /// </summary>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        /// <param name="initialState">The initial state the state machine should enter.</param>
        public void EnterInitialState(
            StateContainer<TState, TEvent> stateContainer,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions,
            TState initialState)
        {
            stateContainer.ForEach(extension => extension.EnteringInitialState(initialState));

            var context = this.factory.CreateTransitionContext(null, new Missable<TEvent>(), Missing.Value, this);
            this.EnterInitialState(context, stateContainer, stateDefinitions, initialState);

            stateContainer.ForEach(extension => extension.EnteredInitialState(initialState, context));
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        public void Fire(
            TEvent eventId,
            StateContainer<TState, TEvent> stateContainer,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            this.Fire(eventId, Missing.Value, stateContainer, stateDefinitions);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        /// <param name="stateContainer">Contains all mutable state of of the state machine.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        public void Fire(
            TEvent eventId,
            object eventArgument,
            StateContainer<TState, TEvent> stateContainer,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            CheckThatStateMachineHasEnteredInitialState(stateContainer);

            stateContainer.ForEach(extension => extension.FiringEvent(ref eventId, ref eventArgument));

            var currentState = stateContainer
                .CurrentStateId
                .Map(x => stateDefinitions[x])
                .ExtractOrThrow();
            var context = this.factory.CreateTransitionContext(currentState, new Missable<TEvent>(eventId), eventArgument, this);
            var result = this.stateLogic.Fire(currentState, context, stateContainer, stateDefinitions);

            if (!result.Fired)
            {
                this.OnTransitionDeclined(context);
                return;
            }

            var newState = stateDefinitions[result.NewState];
            SwitchStateTo(newState, stateContainer, stateDefinitions);

            stateContainer.ForEach(extension => extension.FiredEvent(context));

            this.OnTransitionCompleted(context, stateContainer.CurrentStateId.ExtractOrThrow());
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

        private void OnTransitionCompleted(ITransitionContext<TState, TEvent> transitionContext, TState currentStateId)
        {
            this.RaiseEvent(
                this.TransitionCompleted,
                new TransitionCompletedEventArgs<TState, TEvent>(
                    currentStateId,
                    transitionContext),
                transitionContext,
                true);
        }

        private void EnterInitialState(
            ITransitionContext<TState, TEvent> context,
            StateContainer<TState, TEvent> stateContainer,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions,
            TState initialStateId)
        {
            var initialState = stateDefinitions[initialStateId];
            var initializer = this.factory.CreateStateMachineInitializer(initialState, context);
            var newStateId = initializer.EnterInitialState(this.stateLogic, stateContainer, stateDefinitions);
            var newStateDefinition = stateDefinitions[newStateId];
            SwitchStateTo(newStateDefinition, stateContainer, stateDefinitions);
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

        private static void CheckThatStateMachineHasEnteredInitialState(StateContainer<TState, TEvent> stateContainer)
        {
            if (!stateContainer.CurrentStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineHasNotYetEnteredInitialState);
            }
        }
    }
}