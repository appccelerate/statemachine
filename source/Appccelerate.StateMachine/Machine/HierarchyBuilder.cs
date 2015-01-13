//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilder.cs" company="Appccelerate">
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
    
    using Appccelerate.StateMachine.Syntax;

    public class HierarchyBuilder<TState, TEvent> : 
        IHierarchySyntax<TState>, 
        IInitialSubStateSyntax<TState>,
        ISubStateSyntax<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionary<TState, TEvent> states;

        private readonly IState<TState, TEvent> superState;

        public HierarchyBuilder(IStateDictionary<TState, TEvent> states, TState superStateId)
        {
            Ensure.ArgumentNotNull(states, "states");

            this.states = states;
            this.superState = this.states[superStateId];
        }

        public IInitialSubStateSyntax<TState> WithHistoryType(HistoryType historyType)
        {
            this.superState.HistoryType = historyType;

            return this;
        }

        public ISubStateSyntax<TState> WithInitialSubState(TState stateId)
        {
            this.WithSubState(stateId);

            this.superState.InitialState = this.states[stateId];

            return this;
        }

        public ISubStateSyntax<TState> WithSubState(TState stateId)
        {
            var subState = this.states[stateId];

            this.CheckThatStateHasNotAlreadyASuperState(subState);
            
            subState.SuperState = this.superState;
            this.superState.SubStates.Add(subState);

            return this;
        }

        private void CheckThatStateHasNotAlreadyASuperState(IState<TState, TEvent> subState)
        {
            Ensure.ArgumentNotNull(subState, "subState");

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