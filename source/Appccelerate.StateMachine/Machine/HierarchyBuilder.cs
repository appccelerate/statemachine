//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilder.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Machine.Building;
    using States;
    using Syntax;

    public class HierarchyBuilder<TState, TEvent> :
        IHierarchySyntax<TState>,
        IInitialSubStateSyntax<TState>,
        ISubStateSyntax<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly BuildableStateDefinition<TState, TEvent> superState;

        private readonly ImplicitAddIfNotAvailableStateDefinitionDictionary<TState, TEvent> stateDefinitions;
        private readonly IDictionary<TState, TState> initiallyLastActiveStates;

        public HierarchyBuilder(
            TState superStateId,
            ImplicitAddIfNotAvailableStateDefinitionDictionary<TState, TEvent> stateDefinitions,
            IDictionary<TState, TState> initiallyLastActiveStates)
        {
            Guard.AgainstNullArgument("states", stateDefinitions);

            this.stateDefinitions = stateDefinitions;
            this.initiallyLastActiveStates = initiallyLastActiveStates;
            this.superState = this.stateDefinitions[superStateId];
        }

        public IInitialSubStateSyntax<TState> WithHistoryType(HistoryType historyType)
        {
            this.superState.HistoryType = historyType;

            return this;
        }

        public ISubStateSyntax<TState> WithInitialSubState(TState stateId)
        {
            this.WithSubState(stateId);

            this.superState.InitialState = this.stateDefinitions[stateId];
            this.initiallyLastActiveStates.Add(this.superState.Id, stateId);

            return this;
        }

        public ISubStateSyntax<TState> WithSubState(TState stateId)
        {
            var subState = this.stateDefinitions[stateId];

            this.CheckThatStateHasNotAlreadyASuperState(subState);

            subState.SuperState = this.superState;
            this.superState.SubStates.Add(subState);

            return this;
        }

        private void CheckThatStateHasNotAlreadyASuperState(BuildableStateDefinition<TState, TEvent> subState)
        {
            Guard.AgainstNullArgument("subState", subState);

            if (subState.SuperState != null)
            {
                throw new InvalidOperationException(
                    ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                        this.superState.Id,
                        subState));
            }
        }
    }
}