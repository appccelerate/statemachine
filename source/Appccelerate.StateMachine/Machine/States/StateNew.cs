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
    using System.Collections.Generic;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class StateNew<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Collection of the sub-states of this state.
        /// </summary>
        private readonly List<StateNew<TState, TEvent>> subStates;

        /// <summary>
        /// Collection of transitions that start in this state (<see cref="ITransition{TState,TEvent}.Source"/> is equal to this state).
        /// </summary>
        private readonly TransitionDictionaryNew<TState, TEvent> transitions;

        /// <summary>
        /// The level of this state within the state hierarchy [1..maxLevel].
        /// </summary>
        private int level;

        /// <summary>
        /// The super-state of this state. Null for states with <see cref="level"/> equal to 1.
        /// </summary>
        private StateNew<TState, TEvent> superState;

        /// <summary>
        /// The initial sub-state of this state.
        /// </summary>
        private StateNew<TState, TEvent> initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateNew&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="id">The unique id of this state.</param>
        public StateNew(TState id)
        {
            this.Id = id;
            this.level = 1;

            this.subStates = new List<StateNew<TState, TEvent>>();
            this.transitions = new TransitionDictionaryNew<TState, TEvent>(this);

            this.EntryActions = new List<IActionHolder>();
            this.ExitActions = new List<IActionHolder>();
        }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        public StateNew<TState, TEvent> LastActiveState { get; set; }

        /// <summary>
        /// Gets the unique id of this state.
        /// </summary>
        /// <value>The id of this state.</value>
        public TState Id { get; private set; }

        /// <summary>
        /// Gets the entry actions.
        /// </summary>
        /// <value>The entry actions.</value>
        public IList<IActionHolder> EntryActions { get; private set; }

        /// <summary>
        /// Gets the exit actions.
        /// </summary>
        /// <value>The exit action.</value>
        public IList<IActionHolder> ExitActions { get; private set; }

        /// <summary>
        /// Gets or sets the initial sub state of this state.
        /// </summary>
        /// <value>The initial sub state of this state.</value>
        public StateNew<TState, TEvent> InitialState
        {
            get
            {
                return this.initialState;
            }

            set
            {
                this.CheckInitialStateIsNotThisInstance(value);
                this.CheckInitialStateIsASubState(value);

                this.initialState = this.LastActiveState = value;
            }
        }

        /// <summary>
        /// Gets or sets the super-state of this state.
        /// </summary>
        /// <remarks>
        /// The <see cref="Level"/> of this state is changed accordingly to the super-state.
        /// </remarks>
        /// <value>The super-state of this super.</value>
        public StateNew<TState, TEvent> SuperState
        {
            get
            {
                return this.superState;
            }

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
            get
            {
                return this.level;
            }

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
        public ICollection<StateNew<TState, TEvent>> SubStates
        {
            get { return this.subStates; }
        }

        /// <summary>
        /// Gets the transitions that start in this state.
        /// </summary>
        /// <value>The transitions.</value>
        public ITransitionDictionaryNew<TState, TEvent> Transitions
        {
            get { return this.transitions; }
        }

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
            foreach (var state in this.subStates)
            {
                state.Level = this.level + 1;
            }
        }

        /// <summary>
        /// Throws an exception if the new super state is this instance.
        /// </summary>
        /// <param name="newSuperState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckSuperStateIsNotThisInstance(StateNew<TState, TEvent> newSuperState)
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
        private void CheckInitialStateIsNotThisInstance(StateNew<TState, TEvent> newInitialState)
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
        private void CheckInitialStateIsASubState(StateNew<TState, TEvent> value)
        {
            if (value.SuperState != this)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(value.ToString(), this.ToString()));
            }
        }
    }
}