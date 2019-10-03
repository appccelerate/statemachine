//-------------------------------------------------------------------------------
// <copyright file="InternalExtensionBase.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Extensions
{
    using System;
    using System.Collections.Generic;
    using Infrastructure;
    using Machine;
    using Machine.States;
    using Machine.Transitions;

    public class InternalExtensionBase<TState, TEvent> : IExtensionInternal<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        public virtual void StartedStateMachine()
        {
        }

        public virtual void StoppedStateMachine()
        {
        }

        public virtual void EventQueued(TEvent eventId, object eventArgument)
        {
        }

        public virtual void EventQueuedWithPriority(TEvent eventId, object eventArgument)
        {
        }

        public virtual void SwitchedState(IStateDefinition<TState, TEvent> oldState, IStateDefinition<TState, TEvent> newState)
        {
        }

        public virtual void EnteringInitialState(TState state)
        {
        }

        public virtual void EnteredInitialState(TState state, ITransitionContext<TState, TEvent> context)
        {
        }

        public virtual void FiringEvent(ref TEvent eventId, ref object eventArgument)
        {
        }

        public virtual void FiredEvent(ITransitionContext<TState, TEvent> context)
        {
        }

        public virtual void HandlingEntryActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ref Exception exception)
        {
        }

        public virtual void HandledEntryActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
        }

        public virtual void HandlingExitActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ref Exception exception)
        {
        }

        public virtual void HandledExitActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
        }

        public virtual void HandlingGuardException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext,
            ref Exception exception)
        {
        }

        public virtual void HandledGuardException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext,
            Exception exception)
        {
        }

        public virtual void HandlingTransitionException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context,
            ref Exception exception)
        {
        }

        public virtual void HandledTransitionException(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext,
            Exception exception)
        {
        }

        public virtual void SkippedTransition(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context)
        {
        }

        public virtual void ExecutingTransition(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext)
        {
        }

        public virtual void ExecutedTransition(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> transitionContext)
        {
        }

        public virtual void Loaded(
            IInitializable<TState> loadedCurrentState,
            IReadOnlyDictionary<TState, TState> loadedHistoryStates,
            IReadOnlyCollection<EventInformation<TEvent>> events)
        {
        }

        public void EnteringState(IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context)
        {
        }
    }
}