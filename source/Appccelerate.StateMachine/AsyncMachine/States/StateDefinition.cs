//-------------------------------------------------------------------------------
// <copyright file="StateDefinition.cs" company="Appccelerate">
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
    using System.Collections.Generic;

    using ActionHolders;
    using Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class StateDefinition<TState, TEvent>
        : IStateDefinition<TState, TEvent>
        where TState : notnull
        where TEvent : notnull
    {
        public StateDefinition(
            TState id,
            int level,
            HistoryType historyType,
            IEnumerable<IStateDefinition<TState, TEvent>> subStates,
            IStateDefinition<TState, TEvent>? initialState,
            IEnumerable<IActionHolder> entryActions,
            IEnumerable<IActionHolder> exitActions)
        {
            this.Id = id;
            this.Level = level;
            this.InitialState = initialState;
            this.HistoryType = historyType;
            this.SubStates = subStates;
            this.EntryActions = entryActions;
            this.ExitActions = exitActions;
        }

        /// <summary>
        /// Gets the unique id of this state.
        /// </summary>
        /// <value>The id of this state.</value>
        public TState Id { get; }

        /// <summary>
        /// Gets the level of this state in the state hierarchy.
        /// </summary>
        /// <value>The level.</value>
        public int Level { get; }

        public IReadOnlyDictionary<TEvent, IEnumerable<ITransitionDefinition<TState, TEvent>>> Transitions { get; set; } = new Dictionary<TEvent, IEnumerable<ITransitionDefinition<TState, TEvent>>>();

        public IStateDefinition<TState, TEvent>? InitialState { get; }

        public HistoryType HistoryType { get; }

        public IStateDefinition<TState, TEvent>? SuperState { get; set; }

        public IEnumerable<IStateDefinition<TState, TEvent>> SubStates { get; }

        public IEnumerable<IActionHolder> EntryActions { get; }

        public IEnumerable<IActionHolder> ExitActions { get; }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}