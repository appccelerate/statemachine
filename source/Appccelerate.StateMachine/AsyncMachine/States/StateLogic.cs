//-------------------------------------------------------------------------------
// <copyright file="StateLogic.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.States
{
    using System;
    using System.Threading.Tasks;
    using ActionHolders;
    using Transitions;

    public class StateLogic<TState, TEvent>
        : IStateLogic<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IExtensionHost<TState, TEvent> extensionHost;
        private readonly ITransitionLogic<TState, TEvent> transitionLogic;

        public StateLogic(
            ITransitionLogic<TState, TEvent> transitionLogic,
            IExtensionHost<TState, TEvent> extensionHost)
        {
            this.extensionHost = extensionHost;
            this.transitionLogic = transitionLogic;
        }

        /// <summary>
        /// Goes recursively up the state hierarchy until a state is found that can handle the event.
        /// </summary>
        /// <param name="stateDefinition">The state definition of the state in which the event should be fired.</param>
        /// <param name="context">The event context.</param>
        /// <param name="lastActiveStateModifier">The last active state modifier.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        /// <returns>The result of the transition.</returns>
        public async Task<ITransitionResult<TState>> Fire(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            Guard.AgainstNullArgument("context", context);

            var result = TransitionResult<TState>.NotFired;

            if (stateDefinition.Transitions.TryGetValue(context.EventId.Value, out var transitionsForEvent))
            {
                foreach (var transitionDefinition in transitionsForEvent)
                {
                    result = await this.transitionLogic.Fire(transitionDefinition, context, lastActiveStateModifier, stateDefinitions)
                        .ConfigureAwait(false);
                    if (result.Fired)
                    {
                        return result;
                    }
                }
            }

            if (stateDefinition.SuperState != null)
            {
                result = await this.Fire(stateDefinition.SuperState, context, lastActiveStateModifier, stateDefinitions)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task Entry(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(stateDefinition.Id, RecordType.Enter);

            await this.extensionHost.ForEach(
                extension =>
                    extension.EnteringState(stateDefinition, context));

            await this.ExecuteEntryActions(stateDefinition, context).ConfigureAwait(false);
        }

        public async Task Exit(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(stateDefinition.Id, RecordType.Exit);

            await this.ExecuteExitActions(stateDefinition, context).ConfigureAwait(false);
            this.SetThisStateAsLastStateOfSuperState(stateDefinition, lastActiveStateModifier);
        }

        public async Task<TState> EnterByHistory(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            var result = stateDefinition.Id;

            switch (stateDefinition.HistoryType)
            {
                case HistoryType.None:
                    result = await this.EnterHistoryNone(stateDefinition, context)
                        .ConfigureAwait(false);
                    break;

                case HistoryType.Shallow:
                    result = await this.EnterHistoryShallow(stateDefinition, context, lastActiveStateModifier, stateDefinitions)
                        .ConfigureAwait(false);
                    break;

                case HistoryType.Deep:
                    result = await this.EnterHistoryDeep(stateDefinition, context, lastActiveStateModifier, stateDefinitions)
                        .ConfigureAwait(false);
                    break;
            }

            return result;
        }

        public async Task<TState> EnterShallow(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            await this.Entry(stateDefinition, context).ConfigureAwait(false);

            var initialState = stateDefinition.InitialState;
            if (initialState == null)
            {
                return stateDefinition.Id;
            }

            return await this.EnterShallow(initialState, context).ConfigureAwait(false);
        }

        public async Task<TState> EnterDeep(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            await this.Entry(stateDefinition, context).ConfigureAwait(false);

            var lastActiveStateId = lastActiveStateModifier.GetLastActiveStateFor(stateDefinition.Id);
            if (!lastActiveStateId.HasValue)
            {
                return stateDefinition.Id;
            }

            var lastActiveState = stateDefinitions[lastActiveStateId.Value];
            return await this.EnterDeep(lastActiveState, context, lastActiveStateModifier, stateDefinitions)
                .ConfigureAwait(false);
        }

        private static void HandleException(Exception exception, ITransitionContext<TState, TEvent> context)
        {
            context.OnExceptionThrown(exception);
        }

        private async Task ExecuteEntryActions(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in stateDefinition.EntryActions)
            {
                await this.ExecuteEntryAction(stateDefinition, actionHolder, context)
                    .ConfigureAwait(false);
            }
        }

        private async Task ExecuteEntryAction(
            IStateDefinition<TState, TEvent> stateDefinition,
            IActionHolder actionHolder,
            ITransitionContext<TState, TEvent> context)
        {
            try
            {
                await actionHolder
                    .Execute(context.EventArgument)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await this.HandleEntryActionException(stateDefinition, context, exception)
                    .ConfigureAwait(false);
            }
        }

        private async Task HandleEntryActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandlingEntryActionException(stateDefinition, context, ref exception))
                .ConfigureAwait(false);

            HandleException(exception, context);

            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandledEntryActionException(stateDefinition, context, exception))
                .ConfigureAwait(false);
        }

        private async Task ExecuteExitActions(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in stateDefinition.ExitActions)
            {
                await this.ExecuteExitAction(stateDefinition, actionHolder, context)
                    .ConfigureAwait(false);
            }
        }

        private async Task ExecuteExitAction(
            IStateDefinition<TState, TEvent> stateDefinition,
            IActionHolder actionHolder,
            ITransitionContext<TState, TEvent> context)
        {
            try
            {
                await actionHolder
                    .Execute(context.EventArgument)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await this.HandleExitActionException(stateDefinition, context, exception)
                    .ConfigureAwait(false);
            }
        }

        private async Task HandleExitActionException(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            Exception exception)
        {
            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandlingExitActionException(stateDefinition, context, ref exception))
                .ConfigureAwait(false);

            HandleException(exception, context);

            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandledExitActionException(stateDefinition, context, exception))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets this instance as the last state of this instance's super state.
        /// </summary>
        private void SetThisStateAsLastStateOfSuperState(IStateDefinition<TState, TEvent> stateDefinition, ILastActiveStateModifier<TState> lastActiveStateModifier)
        {
            if (stateDefinition.SuperState != null)
            {
                lastActiveStateModifier.SetLastActiveStateFor(stateDefinition.SuperState.Id, stateDefinition.Id);
            }
        }

        private async Task<TState> EnterHistoryDeep(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            var lastActiveStateId = lastActiveStateModifier.GetLastActiveStateFor(stateDefinition.Id);
            if (!lastActiveStateId.HasValue)
            {
                return stateDefinition.Id;
            }

            var lastActiveState = stateDefinitions[lastActiveStateId.Value];
            return await this.EnterDeep(lastActiveState, context, lastActiveStateModifier, stateDefinitions)
                .ConfigureAwait(false);
        }

        private async Task<TState> EnterHistoryShallow(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            var lastActiveStateId = lastActiveStateModifier.GetLastActiveStateFor(stateDefinition.Id);
            if (!lastActiveStateId.HasValue)
            {
                return stateDefinition.Id;
            }

            var lastActiveState = stateDefinitions[lastActiveStateId.Value];
            return await this.EnterShallow(lastActiveState, context)
                .ConfigureAwait(false);
        }

        private async Task<TState> EnterHistoryNone(
            IStateDefinition<TState, TEvent> stateDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            if (stateDefinition.InitialState != null)
            {
                return await this.EnterShallow(stateDefinition.InitialState, context)
                    .ConfigureAwait(false);
            }

            return stateDefinition.Id;
        }
    }
}
