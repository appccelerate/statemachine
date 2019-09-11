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
    using System;
    using System.Collections.Generic;

    using ActionHolders;
    using Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class StateDefinition<TState, TEvent> : IStateDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Collection of transitions that start in this state (<see cref="ITransitionDefinition{TState,TEvent}.Source"/> is equal to this state).
        /// </summary>
        private readonly TransitionDictionary<TState, TEvent> transitions;

        /// <summary>
        /// The level of this state within the state hierarchy [1..maxLevel].
        /// </summary>
        private int level;

        /// <summary>
        /// The super-state of this state. Null for states with <see cref="level"/> equal to 1.
        /// </summary>
        private StateDefinition<TState, TEvent> superState;

        /// <summary>
        /// The initial sub-state of this state.
        /// </summary>
        private StateDefinition<TState, TEvent> initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateDefinition{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="id">The unique id of this state.</param>
        public StateDefinition(TState id)
        {
            this.Id = id;
            this.level = 1;

            this.transitions = new TransitionDictionary<TState, TEvent>(this);
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
        public IList<IActionHolder> EntryActionsModifiable { get; } = new List<IActionHolder>();

        /// <summary>
        /// Gets the exit actions.
        /// </summary>
        /// <value>The exit action.</value>
        public IList<IActionHolder> ExitActionsModifiable { get; } = new List<IActionHolder>();

        /// <summary>
        /// Gets or sets the initial sub state of this state.
        /// </summary>
        /// <value>The initial sub state of this state.</value>
        public StateDefinition<TState, TEvent> InitialStateModifiable
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
        public StateDefinition<TState, TEvent> SuperStateModifiable
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
        public HistoryType HistoryTypeModifiable { get; set; } = HistoryType.None;

        /// <summary>
        /// Gets the sub-states of this state.
        /// </summary>
        /// <value>The sub-states of this state.</value>
        public ICollection<StateDefinition<TState, TEvent>> SubStatesModifiable { get; } = new List<StateDefinition<TState, TEvent>>();

        /// <summary>
        /// Gets the transitions that start in this state.
        /// </summary>
        /// <value>The transitions.</value>
        public ITransitionDictionary<TState, TEvent> TransitionsModifiable => this.transitions;

        public override string ToString()
        {
            return this.Id.ToString();
        }

        /// <summary>
        /// Sets the initial level depending on the level of the super state of this instance.
        /// </summary>
        private void SetInitialLevel()
        {
            this.Level = this.superState != null ? this.superState.Level + 1 : 1;
        }

        /// <summary>
        /// Sets the level of all sub states.
        /// </summary>
        private void SetLevelOfSubStates()
        {
            foreach (var state in this.SubStatesModifiable)
            {
                state.Level = this.level + 1;
            }
        }

        /// <summary>
        /// Throws an exception if the new super state is this instance.
        /// </summary>
        /// <param name="newSuperState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckSuperStateIsNotThisInstance(StateDefinition<TState, TEvent> newSuperState)
        {
            if (this == newSuperState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeItsOwnSuperState(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is this instance.
        /// </summary>
        /// <param name="newInitialState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckInitialStateIsNotThisInstance(StateDefinition<TState, TEvent> newInitialState)
        {
            if (this == newInitialState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialSubStateToItself(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is not a sub-state of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckInitialStateIsASubState(StateDefinition<TState, TEvent> value)
        {
            if (value.SuperState != this)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(value.ToString(), this.ToString()));
            }
        }

        public IReadOnlyDictionary<TEvent, IEnumerable<ITransitionDefinition<TState, TEvent>>> Transitions => this.transitions.Transitions;

        public IEnumerable<TransitionInfo<TState, TEvent>> TransitionInfos => this.transitions.GetTransitions();

        public IStateDefinition<TState, TEvent> InitialState => this.InitialStateModifiable;

        public HistoryType HistoryType => this.HistoryTypeModifiable;

        public IStateDefinition<TState, TEvent> SuperState => this.SuperStateModifiable;

        public IEnumerable<IStateDefinition<TState, TEvent>> SubStates => this.SubStatesModifiable;

        public IEnumerable<IActionHolder> EntryActions => this.EntryActionsModifiable;

        public IEnumerable<IActionHolder> ExitActions => this.ExitActionsModifiable;
    }
}