// <copyright file="BuildableStateDefinition.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Building
{
    using System;
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Machine.ActionHolders;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class BuildableStateDefinition<TState, TEvent>
        where TState : notnull
        where TEvent : notnull
    {
        /// <summary>
        /// The level of this state within the state hierarchy [1..maxLevel].
        /// </summary>
        private int level;

        /// <summary>
        /// The super-state of this state. Null for states with <see cref="level"/> equal to 1.
        /// </summary>
        private BuildableStateDefinition<TState, TEvent>? superState;

        /// <summary>
        /// The initial sub-state of this state.
        /// </summary>
        private BuildableStateDefinition<TState, TEvent>? initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildableStateDefinition{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="id">The unique id of this state.</param>
        public BuildableStateDefinition(
            TState id)
        {
            this.Id = id;
            this.level = 1;

            this.Transitions = new BuildableTransitionDictionary<TState, TEvent>(this);
        }

        /// <summary>
        /// Gets the unique id of this state.
        /// </summary>
        /// <value>The id of this state.</value>
        public TState Id { get; }

        /// <summary>
        /// Gets the entry actions.
        /// </summary>
        /// <value>The entry actions.</value>
        public IList<IActionHolder> EntryActions { get; } = new List<IActionHolder>();

        /// <summary>
        /// Gets the exit actions.
        /// </summary>
        /// <value>The exit action.</value>
        public IList<IActionHolder> ExitActions { get; } = new List<IActionHolder>();

        /// <summary>
        /// Gets or sets the initial sub state of this state.
        /// </summary>
        /// <value>The initial sub state of this state.</value>
        public BuildableStateDefinition<TState, TEvent>? InitialState
        {
            get => this.initialState;
            set
            {
                this.CheckInitialStateIsNotThisInstance(value);
                this.CheckInitialStateIsASubState(value);

                this.initialState = value;
            }
        }

        /// <summary>
        /// Gets or sets the super-state of this state.
        /// </summary>
        /// <remarks>
        /// The <see cref="Level"/> of this state is changed accordingly to the super-state.
        /// </remarks>
        /// <value>The super-state of this super.</value>
        public BuildableStateDefinition<TState, TEvent>? SuperState
        {
            get => this.superState;
            set
            {
                this.CheckSuperStateIsNotThisInstance(value);

                this.superState = value;

                this.SetInitialLevel();
            }
        }

        /// <summary>
        /// Gets or sets the level of this state in the state hierarchy.
        /// When set then the levels of all sub-states are changed accordingly.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get => this.level;
            set
            {
                this.level = value;

                this.SetLevelOfSubStates();
            }
        }

        /// <summary>
        /// Gets or sets the history type of this state.
        /// </summary>
        /// <value>The type of the history.</value>
        public HistoryType HistoryType { get; set; } = HistoryType.None;

        /// <summary>
        /// Gets the sub-states of this state.
        /// </summary>
        /// <value>The sub-states of this state.</value>
        public ICollection<BuildableStateDefinition<TState, TEvent>> SubStates { get; } = new List<BuildableStateDefinition<TState, TEvent>>();

        /// <summary>
        /// Gets the transitions that start in this state.
        /// </summary>
        /// <value>The transitions.</value>
        public BuildableTransitionDictionary<TState, TEvent> Transitions { get; }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        /// <summary>
        /// Sets the initial level depending on the level of the super state of this instance.
        /// </summary>
        private void SetInitialLevel()
        {
            this.Level = this.superState?.Level + 1 ?? 1;
        }

        /// <summary>
        /// Sets the level of all sub states.
        /// </summary>
        private void SetLevelOfSubStates()
        {
            foreach (var state in this.SubStates)
            {
                state.Level = this.level + 1;
            }
        }

        /// <summary>
        /// Throws an exception if the new super state is this instance.
        /// </summary>
        /// <param name="newSuperState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckSuperStateIsNotThisInstance(
            BuildableStateDefinition<TState, TEvent>? newSuperState)
        {
            if (this == newSuperState)
            {
                throw new ArgumentException(BuildingExceptionMessages.StateCannotBeItsOwnSuperState(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is this instance.
        /// </summary>
        /// <param name="newInitialState">The value.</param>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckInitialStateIsNotThisInstance(
            BuildableStateDefinition<TState, TEvent>? newInitialState)
        {
            if (this == newInitialState)
            {
                throw new ArgumentException(
                    BuildingExceptionMessages.StateCannotBeTheInitialSubStateToItself(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is not a sub-state of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckInitialStateIsASubState(
            BuildableStateDefinition<TState, TEvent>? value)
        {
            if (value != null && value.SuperState != this)
            {
                throw new ArgumentException(BuildingExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(
                    value.ToString(),
                    this.ToString()));
            }
        }
    }
}