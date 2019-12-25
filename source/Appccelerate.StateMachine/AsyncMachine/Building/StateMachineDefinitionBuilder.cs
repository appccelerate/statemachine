//-------------------------------------------------------------------------------
// <copyright file="StateMachineDefinitionBuilder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Appccelerate.StateMachine.AsyncMachine.States;
    using Appccelerate.StateMachine.AsyncMachine.Transitions;
    using Appccelerate.StateMachine.AsyncSyntax;

    public class StateMachineDefinitionBuilder<TState, TEvent>
        where TState : notnull
        where TEvent : notnull
    {
        private readonly StandardFactory<TState, TEvent> factory = new StandardFactory<TState, TEvent>();
        private readonly ImplicitAddIfNotAvailableStateDefinitionDictionary<TState, TEvent> stateDefinitionDictionary = new ImplicitAddIfNotAvailableStateDefinitionDictionary<TState, TEvent>();
        private readonly Dictionary<TState, BuildableStateDefinition<TState, TEvent>> initiallyLastActiveStates = new Dictionary<TState, BuildableStateDefinition<TState, TEvent>>();
        private Option<TState> initialState = Option<TState>.None;

        public IEntryActionSyntax<TState, TEvent> In(
            TState state)
        {
            return new StateBuilder<TState, TEvent>(
                state,
                this.stateDefinitionDictionary,
                this.factory);
        }

        public IHierarchySyntax<TState> DefineHierarchyOn(
            TState superStateId)
        {
            return new HierarchyBuilder<TState, TEvent>(
                superStateId,
                this.stateDefinitionDictionary,
                this.initiallyLastActiveStates);
        }

        public StateMachineDefinitionBuilder<TState, TEvent> WithInitialState(
            TState initialStateToUse)
        {
            this.initialState = Option<TState>.Some(initialStateToUse);
            return this;
        }

        public StateMachineDefinition<TState, TEvent> Build()
        {
            var safeInitialState = this.initialState.ExtractOrThrow(() =>
                throw new InvalidOperationException(ExceptionMessages.InitialStateNotConfigured));

            var states = this.stateDefinitionDictionary.GetBuildableStateDefinitions();

            var topStates = states
                .Where(x => x.SuperState == null);

            var queue = new Queue<BuildableStateDefinition<TState, TEvent>>(
                topStates);

            var stateDefinitions = new Dictionary<TState, StateDefinition<TState, TEvent>>(states.Count);

            while (queue.Any())
            {
                var state = queue.Dequeue();

                this.BuildStateDefinition(
                    state,
                    newState => stateDefinitions.Add(
                        newState.Id,
                        newState));
            }

            foreach (var state in states)
            {
                var stateDefinition = stateDefinitions[state.Id];

                if (state.SuperState != null)
                {
                    stateDefinition.SuperState = stateDefinitions[state.SuperState.Id];
                }

                var transitionDefinitions = state.Transitions
                    .Select(x => new
                        {
                            SourceId = x.Source != null ? x.Source.Id : throw new Exception("Source of transition is null"),
                            x.Event,
                            Target = x.Target != null ? stateDefinitions[x.Target.Id] : null,
                            x.Guard,
                            x.Actions
                        })
                    .Select(x =>
                        new TransitionDefinition<TState, TEvent>(
                            stateDefinitions[x.SourceId],
                            x.Event,
                            x.Target != null ? stateDefinitions[x.Target.Id] : null,
                            x.Guard,
                            x.Actions))
                    .GroupBy(
                        x => x.Event)
                    .ToDictionary(
                        x => x.Key,
                        x => (IEnumerable<ITransitionDefinition<TState, TEvent>>)x);

                stateDefinition.Transitions = transitionDefinitions;
            }

            return new StateMachineDefinition<TState, TEvent>(
                new StateDefinitionDictionary<TState, TEvent>(
                    stateDefinitions
                        .ToDictionary(
                            pair => pair.Key,
                            pair => (IStateDefinition<TState, TEvent>)pair.Value)),
                this.initiallyLastActiveStates
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Id),
                safeInitialState);
        }

        private StateDefinition<TState, TEvent> BuildStateDefinition(
            BuildableStateDefinition<TState, TEvent> state,
            Action<StateDefinition<TState, TEvent>> addNewState)
        {
            var subStates = new Dictionary<TState, StateDefinition<TState, TEvent>>();
            foreach (var subState in state.SubStates)
            {
                var subStateDefinition = this.BuildStateDefinition(
                    subState,
                    addNewState);

                subStates.Add(
                    subStateDefinition.Id,
                    subStateDefinition);
            }

            var stateDefinition = new StateDefinition<TState, TEvent>(
                state.Id,
                state.Level,
                state.HistoryType,
                subStates.Values,
                state.InitialState != null ? subStates[state.InitialState.Id] : null,
                state.EntryActions,
                state.ExitActions);

            addNewState(stateDefinition);

            return stateDefinition;
        }
    }
}