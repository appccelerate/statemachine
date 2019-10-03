//-------------------------------------------------------------------------------
// <copyright file="TransitionDictionary.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using States;

    /// <summary>
    /// Manages the transitions of a state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class TransitionDictionary<TState, TEvent> : ITransitionDictionary<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// The transitions.
        /// </summary>
        private readonly Dictionary<TEvent, List<TransitionDefinition<TState, TEvent>>> transitions;

        /// <summary>
        /// The state this transition dictionary belongs to.
        /// </summary>
        private readonly IStateDefinition<TState, TEvent> state;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionDictionary&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public TransitionDictionary(IStateDefinition<TState, TEvent> state)
        {
            this.state = state;
            this.transitions = new Dictionary<TEvent, List<TransitionDefinition<TState, TEvent>>>();
        }

        public IReadOnlyDictionary<TEvent, IEnumerable<ITransitionDefinition<TState, TEvent>>> Transitions =>
            this.transitions.ToDictionary(
                pair => pair.Key,
                pair => (IEnumerable<ITransitionDefinition<TState, TEvent>>)pair.Value);

        /// <summary>
        /// Gets the transitions for the specified event id.
        /// </summary>
        /// <value>transitions for the event id.</value>
        /// <param name="eventId">Id of the event.</param>
        /// <returns>The transitions for the event id.</returns>
        public ICollection<TransitionDefinition<TState, TEvent>> this[TEvent eventId]
        {
            get
            {
                this.transitions.TryGetValue(eventId, out var result);
                return result;
            }
        }

        /// <summary>
        /// Adds the specified event id.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="transitionDefinition">The transition.</param>
        public void Add(TEvent eventId, TransitionDefinition<TState, TEvent> transitionDefinition)
        {
            Guard.AgainstNullArgument("transition", transitionDefinition);

            this.CheckTransitionDoesNotYetExist(transitionDefinition);

            transitionDefinition.Source = this.state;

            this.MakeSureEventExistsInTransitionList(eventId);

            this.transitions[eventId].Add(transitionDefinition);
        }

        /// <summary>
        /// Gets all transitions.
        /// </summary>
        /// <returns>All transitions.</returns>
        public IEnumerable<TransitionInfo<TState, TEvent>> GetTransitions()
        {
            return this.transitions
                .SelectMany(eventIdAndStates =>
                    eventIdAndStates.Value.Select(transition =>
                        new TransitionInfo<TState, TEvent>(eventIdAndStates.Key, transition.Source, transition.Target, transition.Guard, transition.Actions)));
        }

        /// <summary>
        /// Throws an exception if the specified transition is already defined on this state.
        /// </summary>
        /// <param name="transitionDefinition">The transition.</param>
        private void CheckTransitionDoesNotYetExist(TransitionDefinition<TState, TEvent> transitionDefinition)
        {
            if (transitionDefinition.Source != null)
            {
                throw new InvalidOperationException(TransitionsExceptionMessages.TransitionDoesAlreadyExist(transitionDefinition, this.state));
            }
        }

        /// <summary>
        /// If there is no entry in the <see cref="transitions"/> dictionary then one is created.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        private void MakeSureEventExistsInTransitionList(TEvent eventId)
        {
            if (this.transitions.ContainsKey(eventId))
            {
                return;
            }

            var list = new List<TransitionDefinition<TState, TEvent>>();
            this.transitions.Add(eventId, list);
        }
    }
}