//-------------------------------------------------------------------------------
// <copyright file="InternalExtension.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure;
    using States;
    using Transitions;

    public class InternalExtension<TState, TEvent> : IExtensionInternal<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IExtension<TState, TEvent> apiExtension;
        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;

        public InternalExtension(
            IExtension<TState, TEvent> apiExtension,
            IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            this.apiExtension = apiExtension;
            this.stateMachineInformation = stateMachineInformation;
        }

        public Task StartedStateMachine()
        {
            return this.apiExtension.StartedStateMachine(this.stateMachineInformation);
        }

        public Task StoppedStateMachine()
        {
            return this.apiExtension.StoppedStateMachine(this.stateMachineInformation);
        }

        public Task EventQueued(TEvent eventId, object eventArgument)
        {
            return this.apiExtension.EventQueued(this.stateMachineInformation, eventId, eventArgument);
        }

        public Task EventQueuedWithPriority(TEvent eventId, object eventArgument)
        {
            return this.apiExtension.EventQueuedWithPriority(this.stateMachineInformation, eventId, eventArgument);
        }

        public Task SwitchedState(IStateDefinition<TState, TEvent> oldState, IStateDefinition<TState, TEvent> newState)
        {
            return this.apiExtension.SwitchedState(this.stateMachineInformation, oldState, newState);
        }

        public Task EnteringInitialState(TState state)
        {
            return this.apiExtension.EnteringInitialState(this.stateMachineInformation, state);
        }

        public Task EnteredInitialState(TState state, ITransitionContext<TState, TEvent> context)
        {
            return this.apiExtension.EnteredInitialState(this.stateMachineInformation, state, context);
        }

        public Task FiringEvent(ref TEvent eventId, ref object eventArgument)
        {
            return this.apiExtension.FiringEvent(this.stateMachineInformation, ref eventId, ref eventArgument);
        }

        public Task FiredEvent(ITransitionContext<TState, TEvent> context)
        {
            return this.apiExtension.FiredEvent(this.stateMachineInformation, context);
        }

        public Task HandlingEntryActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ref Exception exception)
        {
            return this.apiExtension.HandlingEntryActionException(this.stateMachineInformation, stateDefinition, context, ref exception);
        }

        public Task HandledEntryActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
            return this.apiExtension.HandledEntryActionException(this.stateMachineInformation, stateDefinition, context, exception);
        }

        public Task HandlingExitActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ref Exception exception)
        {
            return this.apiExtension.HandlingExitActionException(this.stateMachineInformation, stateDefinition, context, ref exception);
        }

        public Task HandledExitActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
            return this.apiExtension.HandledExitActionException(this.stateMachineInformation, stateDefinition, context, exception);
        }

        public Task HandlingGuardException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext,
            ref Exception exception)
        {
            return this.apiExtension.HandlingGuardException(this.stateMachineInformation, transitionDefinition, transitionContext, ref exception);
        }

        public Task HandledGuardException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext,
            Exception exception)
        {
            return this.apiExtension.HandledGuardException(this.stateMachineInformation, transitionDefinition, transitionContext, exception);
        }

        public Task HandlingTransitionException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context,
            ref Exception exception)
        {
            return this.apiExtension.HandlingTransitionException(this.stateMachineInformation, transitionDefinition, context, ref exception);
        }

        public Task HandledTransitionException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext,
            Exception exception)
        {
            return this.apiExtension.HandledTransitionException(this.stateMachineInformation, transitionDefinition, transitionContext, exception);
        }

        public Task SkippedTransition(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            return this.apiExtension.SkippedTransition(this.stateMachineInformation, transitionDefinition, context);
        }

        public Task ExecutingTransition(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext)
        {
            return this.apiExtension.ExecutingTransition(this.stateMachineInformation, transitionDefinition, transitionContext);
        }

        public Task ExecutedTransition(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext)
        {
            return this.apiExtension.ExecutedTransition(this.stateMachineInformation, transitionDefinition, transitionContext);
        }

        public Task Loaded(
            IInitializable<TState> loadedCurrentState,
            IReadOnlyDictionary<TState, TState> loadedHistoryStates,
            IReadOnlyCollection<EventInformation<TEvent>> events,
            IReadOnlyCollection<EventInformation<TEvent>> priorityEvents)
        {
            return this.apiExtension.Loaded(this.stateMachineInformation, loadedCurrentState, loadedHistoryStates, events, priorityEvents);
        }

        public Task EnteringState(IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context)
        {
            return this.apiExtension.EnteringState(this.stateMachineInformation, stateDefinition, context);
        }
    }
}