//-------------------------------------------------------------------------------
// <copyright file="IExtension.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

    /// <summary>
    /// Extensions for a state machine have to implement this interface.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IExtension<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Called after the state machine was started.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        void StartedStateMachine(IStateMachineInformation<TState, TEvent> stateMachine);

        /// <summary>
        /// Called after the state machine stopped.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        void StoppedStateMachine(IStateMachineInformation<TState, TEvent> stateMachine);

        /// <summary>
        /// Called after an events was queued.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventArgument">The event argument.</param>
        void EventQueued(IStateMachineInformation<TState, TEvent> stateMachine, TEvent eventId, object eventArgument);

        /// <summary>
        /// Called after an events was queued with priority.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventArgument">The event argument.</param>
        void EventQueuedWithPriority(IStateMachineInformation<TState, TEvent> stateMachine, TEvent eventId, object eventArgument);
        
        /// <summary>
        /// Called after the state machine switched states.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="oldState">The old state.</param>
        /// <param name="newState">The new state.</param>
        void SwitchedState(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            IState<TState, TEvent> oldState, 
            IState<TState, TEvent> newState);

        /// <summary>
        /// Called when the state machine is initializing.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="initialState">The initial state. Can be replaced by the extension.</param>
        void InitializingStateMachine(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ref TState initialState);

        /// <summary>
        /// Called when the state machine was initialized.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="initialState">The initial state.</param>
        void InitializedStateMachine(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            TState initialState);

        /// <summary>
        /// Called when the state machine enters the initial state.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        void EnteringInitialState(IStateMachineInformation<TState, TEvent> stateMachine, TState state);

        /// <summary>
        /// Called when the state machine entered the initial state.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        /// <param name="context">The context.</param>
        void EnteredInitialState(IStateMachineInformation<TState, TEvent> stateMachine, TState state, ITransitionContext<TState, TEvent> context);

        /// <summary>
        /// Called when an event is firing on the state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="eventId">The event id. Can be replaced by the extension.</param>
        /// <param name="eventArgument">The event argument. Can be replaced by the extension.</param>
        void FiringEvent(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ref TEvent eventId, 
            ref object eventArgument);

        /// <summary>
        /// Called when an event was fired on the state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="context">The transition context.</param>
        void FiredEvent(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ITransitionContext<TState, TEvent> context);

        /// <summary>
        /// Called before an entry action exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception. Can be replaced by the extension.</param>
        void HandlingEntryActionException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            IState<TState, TEvent> state, 
            ITransitionContext<TState, TEvent> context, 
            ref Exception exception);

        /// <summary>
        /// Called after an entry action exception was handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        void HandledEntryActionException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            IState<TState, TEvent> state, 
            ITransitionContext<TState, TEvent> context, 
            Exception exception);

        /// <summary>
        /// Called before an exit action exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception. Can be replaced by the extension.</param>
        void HandlingExitActionException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            IState<TState, TEvent> state, 
            ITransitionContext<TState, TEvent> context, 
            ref Exception exception);

        /// <summary>
        /// Called after an exit action exception was handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        void HandledExitActionException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            IState<TState, TEvent> state, 
            ITransitionContext<TState, TEvent> context, 
            Exception exception);

        /// <summary>
        /// Called before a guard exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="transition">The transition.</param>
        /// <param name="transitionContext">The transition context.</param>
        /// <param name="exception">The exception. Can be replaced by the extension.</param>
        void HandlingGuardException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ITransition<TState, TEvent> transition, 
            ITransitionContext<TState, TEvent> transitionContext, 
            ref Exception exception);

        /// <summary>
        /// Called after a guard exception was handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="transition">The transition.</param>
        /// <param name="transitionContext">The transition context.</param>
        /// <param name="exception">The exception.</param>
        void HandledGuardException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ITransition<TState, TEvent> transition, 
            ITransitionContext<TState, TEvent> transitionContext, 
            Exception exception);

        /// <summary>
        /// Called before a transition exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="transition">The transition.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception. Can be replaced by the extension.</param>
        void HandlingTransitionException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ITransition<TState, TEvent> transition, 
            ITransitionContext<TState, TEvent> context, 
            ref Exception exception);

        /// <summary>
        /// Called after a transition exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="transition">The transition.</param>
        /// <param name="transitionContext">The transition context.</param>
        /// <param name="exception">The exception.</param>
        void HandledTransitionException(
            IStateMachineInformation<TState, TEvent> stateMachine, 
            ITransition<TState, TEvent> transition, 
            ITransitionContext<TState, TEvent> transitionContext, 
            Exception exception);

        /// <summary>
        /// Called when a guard of a transition returns false and therefore the transition is not executed.
        /// </summary>
        void SkippedTransition(
            IStateMachineInformation<TState, TEvent> stateMachineInformation, 
            ITransition<TState, TEvent> transition, 
            ITransitionContext<TState, TEvent> context);

        /// <summary>
        /// Called when a transition is going to be executed. After the guard of the transition evaluated to true.
        /// </summary>
        void ExecutingTransition(
            IStateMachineInformation<TState, TEvent> stateMachineInformation,
            ITransition<TState, TEvent> transition,
            ITransitionContext<TState, TEvent> context);

        /// <summary>
        /// Called when a transition was executed.
        /// </summary>
        void ExecutedTransition(
            IStateMachineInformation<TState, TEvent> stateMachineInformation, 
            ITransition<TState, TEvent> transition, 
            ITransitionContext<TState, TEvent> context);
    }
}