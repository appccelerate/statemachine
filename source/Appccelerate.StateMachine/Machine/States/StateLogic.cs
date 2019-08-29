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
    using System.Runtime.InteropServices.ComTypes;
    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Transitions;
    using Events;

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
        private readonly IFactory<TState, TEvent> factory;
        private readonly IExtensionHost<TState, TEvent> extensionHost;
        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;
        private readonly Func<ITransitionLogic<TState, TEvent>> transitionLogicFunc;

        public StateLogic(
            IFactory<TState, TEvent> factory,
            IExtensionHost<TState, TEvent> extensionHost,
            IStateMachineInformation<TState, TEvent> stateMachineInformation,
            Func<ITransitionLogic<TState, TEvent>> transitionLogicFunc)
        {
            this.factory = factory;
            this.extensionHost = extensionHost;
            this.stateMachineInformation = stateMachineInformation;
            this.transitionLogicFunc = transitionLogicFunc;
        }

        /// <summary>
        /// Goes recursively up the state hierarchy until a state is found that can handle the event.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The result of the transition.</returns>
        public ITransitionResult<TState> Fire(ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            ITransitionResult<TState> result = TransitionResult<TState>.NotFired;

            var transitionsForEvent = context.StateDefinition.Transitions[context.EventId.Value];
            if (transitionsForEvent != null)
            {
                foreach (var transitionDefinition in transitionsForEvent)
                {
                    result = this.transitionLogicFunc().Fire(context, transitionDefinition);
                    if (result.Fired)
                    {
                        return result;
                    }
                }
            }

            if (context.StateDefinition.SuperState != null)
            {
                var superStateTransitionContext = this.factory.CreateTransitionContextForSuperStateOf(context);
                result = this.Fire(superStateTransitionContext);
            }

            return result;
        }

        public void Entry(ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(context.StateDefinition.Id, RecordType.Enter);

            this.ExecuteEntryActions(context);
        }

        public void Exit(ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(context.StateDefinition.Id, RecordType.Exit);

            this.ExecuteExitActions(context);
            this.SetThisStateAsLastStateOfSuperState(context);
        }

        public TState EnterByHistory(ITransitionContext<TState, TEvent> context)
        {
            var result = context.StateDefinition.Id;

            switch (context.StateDefinition.HistoryType)
            {
                case HistoryType.None:
                    result = this.EnterHistoryNone(context);
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

        public TState EnterShallow(ITransitionContext<TState, TEvent> context)
        {
            this.Entry(context);

            if (context.StateDefinition.InitialState == null)
            {
                return context.StateDefinition.Id;
            }

            var initialStateTransitionContext = this.factory.CreateTransitionContextForInitialStateOf(context);
            return this.EnterShallow(initialStateTransitionContext);
        }

        public TState EnterDeep(ITransitionContext<TState, TEvent> context)
        {
            this.Entry(context);

            return this.LastActiveState == null ?
                        this :
                        this.LastActiveState.EnterDeep(context);
        }

        private static void HandleException(Exception exception, ITransitionContext<TState, TEvent> context)
        {
            context.OnExceptionThrown(exception);
        }

        private void ExecuteEntryActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in context.StateDefinition.EntryActions)
            {
                this.ExecuteEntryAction(actionHolder, context);
            }
        }

        private void ExecuteEntryAction(IActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            try
            {
                actionHolder.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.HandleEntryActionException(context, exception);
            }
        }

        private void HandleEntryActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingEntryActionException(
                    this.stateMachineInformation, context.StateDefinition.Id, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledEntryActionException(
                    this.stateMachineInformation, context.StateDefinition.Id, context, exception));
        }

        private void ExecuteExitActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in context.StateDefinition.ExitActions)
            {
                this.ExecuteExitAction(actionHolder, context);
            }
        }

        private void ExecuteExitAction(IActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            try
            {
                actionHolder.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.HandleExitActionException(context, exception);
            }
        }

        private void HandleExitActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.extensionHost.ForEach(
                extension =>
                extension.HandlingExitActionException(
                    this.stateMachineInformation, context.StateDefinition.Id, context, ref exception));

            HandleException(exception, context);

            this.extensionHost.ForEach(
                extension =>
                extension.HandledExitActionException(
                    this.stateMachineInformation, context.StateDefinition.Id, context, exception));
        }

        /// <summary>
        /// Sets this instance as the last state of this instance's super state.
        /// </summary>
        private void SetThisStateAsLastStateOfSuperState(ITransitionContext<TState, TEvent> context)
        {
            if (context.StateDefinition.SuperState != null)
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

        private TState EnterHistoryNone(ITransitionContext<TState, TEvent> context)
        {
            if (context.StateDefinition.InitialState != null)
            {
                var initialStateTransitionContext = this.factory.CreateTransitionContextForInitialStateOf(context);
                return this.EnterShallow(initialStateTransitionContext);
            }

            return context.StateDefinition.Id;
        }
    }
}