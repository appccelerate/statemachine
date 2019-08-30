//-------------------------------------------------------------------------------
// <copyright file="State.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Machine.States
{
    using System;
    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class StateLogic<TState, TEvent>
        : IStateLogic<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IExtensionHost<TState, TEvent> extensionHost;
        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;
        private readonly ITransitionLogic<TState, TEvent> transitionLogic;

        public StateLogic(
            ITransitionLogic<TState, TEvent> transitionLogic,
            IExtensionHost<TState, TEvent> extensionHost,
            IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            this.extensionHost = extensionHost;
            this.stateMachineInformation = stateMachineInformation;
            this.transitionLogic = transitionLogic;
        }

        /// <summary>
        /// Goes recursively up the state hierarchy until a state is found that can handle the event.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The result of the transition.</returns>
        public ITransitionResult<TState> Fire(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            ITransitionResult<TState> result = TransitionResult<TState>.NotFired;

            var transitionsForEvent = stateDefinition.Transitions[context.EventId.Value];
            if (transitionsForEvent != null)
            {
                foreach (var transitionDefinition in transitionsForEvent)
                {
                    result = this.transitionLogic.Fire(transitionDefinition, context);
                    if (result.Fired)
                    {
                        return result;
                    }
                }
            }

            if (stateDefinition.SuperState != null)
            {
                result = this.Fire(stateDefinition.SuperState, context);
            }

            return result;
        }

        public void Entry(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(stateDefinition.Id, RecordType.Enter);

            this.ExecuteEntryActions(stateDefinition, context);
        }

        public void Exit(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(stateDefinition.Id, RecordType.Exit);

            this.ExecuteExitActions(stateDefinition, context);
            this.SetThisStateAsLastStateOfSuperState(stateDefinition);
        }

        public TState EnterByHistory(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            var result = stateDefinition.Id;

            switch (stateDefinition.HistoryType)
            {
                case HistoryType.None:
                    result = this.EnterHistoryNone(stateDefinition, context);
                    break;

                case HistoryType.Shallow:
                    result = this.EnterHistoryShallow(context);
                    break;

                case HistoryType.Deep:
                    result = this.EnterHistoryDeep(context);
                    break;
            }

            return result;
        }

        public TState EnterShallow(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            this.Entry(stateDefinition, context);

            if (stateDefinition.InitialState == null)
            {
                return stateDefinition.Id;
            }

            return this.EnterShallow(stateDefinition, context);
        }

        private TState EnterDeep(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            this.Entry(stateDefinition, context);

            return this.LastActiveState == null ?
                        this :
                        this.LastActiveState.EnterDeep(context);
        }

        private static void HandleException(Exception exception, ITransitionContext<TState, TEvent> context)
        {
            context.OnExceptionThrown(exception);
        }

        private void ExecuteEntryActions(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in stateDefinition.EntryActions)
            {
                this.ExecuteEntryAction(stateDefinition, actionHolder, context);
            }
        }

        private void ExecuteEntryAction(
            StateNew<TState, TEvent> stateDefinition,
            IActionHolder actionHolder,
            ITransitionContext<TState, TEvent> context)
        {
            try
            {
                actionHolder.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.HandleEntryActionException(stateDefinition, context, exception);
            }
        }

        private void HandleEntryActionException(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingEntryActionException(
                    this.stateMachineInformation, stateDefinition.Id, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledEntryActionException(
                    this.stateMachineInformation, stateDefinition.Id, context, exception));
        }

        private void ExecuteExitActions(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in stateDefinition.ExitActions)
            {
                this.ExecuteExitAction(stateDefinition, actionHolder, context);
            }
        }

        private void ExecuteExitAction(
            StateNew<TState, TEvent> stateDefinition,
            IActionHolder actionHolder,
            ITransitionContext<TState, TEvent> context)
        {
            try
            {
                actionHolder.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.HandleExitActionException(stateDefinition, context, exception);
            }
        }

        private void HandleExitActionException(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingExitActionException(
                    this.stateMachineInformation, stateDefinition.Id, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledExitActionException(
                    this.stateMachineInformation, stateDefinition.Id, context, exception));
        }

        /// <summary>
        /// Sets this instance as the last state of this instance's super state.
        /// </summary>
        private void SetThisStateAsLastStateOfSuperState(StateNew<TState, TEvent> stateDefinition)
        {
            if (stateDefinition.SuperState != null)
            {
                this.superState.LastActiveState = this;
            }
        }

        private TState EnterHistoryDeep(ITransitionContext<TState, TEvent> context)
        {
            return this.LastActiveState != null
                       ?
                           this.LastActiveState.EnterDeep(context)
                       :
                           this;
        }

        private TState EnterHistoryShallow(ITransitionContext<TState, TEvent> context)
        {
            return this.LastActiveState != null
                       ?
                           this.LastActiveState.EnterShallow(context)
                       :
                           this;
        }

        private TState EnterHistoryNone(
            StateNew<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            if (stateDefinition.InitialState != null)
            {
                return this.EnterShallow(stateDefinition.InitialState, context);
            }

            return stateDefinition.Id;
        }
    }
}