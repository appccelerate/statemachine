//-------------------------------------------------------------------------------
// <copyright file="BuildableTransitionDictionary.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Building
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// Manages the transitions of a state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class BuildableTransitionDictionary<TState, TEvent>
        : IEnumerable<BuildableTransitionDefinition<TState, TEvent>>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly List<BuildableTransitionDefinition<TState, TEvent>> transitions;
        private readonly BuildableStateDefinition<TState, TEvent> state;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionDictionary&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public BuildableTransitionDictionary(BuildableStateDefinition<TState, TEvent> state)
        {
            this.state = state;
            this.transitions = new List<BuildableTransitionDefinition<TState, TEvent>>();
        }

        /// <summary>
        /// Adds the specified event id.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="transitionDefinition">The transition.</param>
        public void Add(
            BuildableTransitionDefinition<TState, TEvent> transitionDefinition)
        {
            Guard.AgainstNullArgument("transition", transitionDefinition);

            this.CheckTransitionDoesNotYetExist(transitionDefinition);

            transitionDefinition.Source = this.state;

            this.transitions.Add(transitionDefinition);
        }

        /// <summary>
        /// Throws an exception if the specified transition is already defined on this state.
        /// </summary>
        /// <param name="transitionDefinition">The transition.</param>
        private void CheckTransitionDoesNotYetExist(BuildableTransitionDefinition<TState, TEvent> transitionDefinition)
        {
            if (transitionDefinition.Source != null)
            {
                throw new InvalidOperationException(TransitionsExceptionMessages.TransitionDoesAlreadyExist(transitionDefinition, this.state));
            }
        }

        public IEnumerator<BuildableTransitionDefinition<TState, TEvent>> GetEnumerator()
        {
            return this.transitions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}