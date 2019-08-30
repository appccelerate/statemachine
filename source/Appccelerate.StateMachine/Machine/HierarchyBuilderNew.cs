//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;

    using Appccelerate.StateMachine.SyntaxNew;
    using States;

    public class HierarchyBuilderNew<TState, TEvent> :
        IHierarchySyntax<TState>,
        IInitialSubStateSyntax<TState>,
        ISubStateSyntax<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StateNew<TState, TEvent> superState;

        private readonly IStateDictionaryNew<TState, TEvent> states;

        public HierarchyBuilderNew(TState superStateId, IStateDictionaryNew<TState, TEvent> states)
        {
            Guard.AgainstNullArgument("states", states);

            this.states = states;
            this.superState = this.states[superStateId];
        }

        public IInitialSubStateSyntax<TState> WithHistoryType(HistoryType historyType)
        {
            this.superState.HistoryTypeModifiable = historyType;

            return this;
        }

        public ISubStateSyntax<TState> WithInitialSubState(TState stateId)
        {
            this.WithSubState(stateId);

            this.superState.InitialStateModifiable = this.states[stateId];

            return this;
        }

        public ISubStateSyntax<TState> WithSubState(TState stateId)
        {
            var subState = this.states[stateId];

            this.CheckThatStateHasNotAlreadyASuperState(subState);

            subState.SuperStateModifiable = this.superState;
            this.superState.SubStatesModifiable.Add(subState);

            return this;
        }

        private void CheckThatStateHasNotAlreadyASuperState(StateNew<TState, TEvent> subState)
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