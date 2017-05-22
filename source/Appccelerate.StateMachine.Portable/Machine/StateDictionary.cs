//-------------------------------------------------------------------------------
// <copyright file="StateDictionary.cs" company="Appccelerate">
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
    using System.Collections.Generic;

    using Appccelerate.StateMachine.Machine.Events;

    /// <summary>
    /// Dictionary mapping state ids to states.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateDictionary<TState, TEvent> : IStateDictionary<TState, TEvent> where TState : IComparable
                                                                                      where TEvent : IComparable
    {
        /// <summary>
        /// Maps ids to states.
        /// </summary>
        private readonly Dictionary<TState, IState<TState, TEvent>> dictionary = new Dictionary<TState, IState<TState, TEvent>>();

        private readonly IFactory<TState, TEvent> factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateDictionary&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public StateDictionary(IFactory<TState, TEvent> factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Gets the <see cref="IState&lt;TState,TEvent&gt;"/> with the specified state id.
        /// </summary>
        /// <value>State with the specified id.</value>
        /// <param name="stateId">The State id.</param>
        /// <returns>The State with the specified id.</returns>
        public IState<TState, TEvent> this[TState stateId]
        {
            get
            {
                if (!this.dictionary.ContainsKey(stateId))
                {
                    this.dictionary.Add(stateId, this.factory.CreateState(stateId));
                }

                return this.dictionary[stateId];
            }
        }

        /// <summary>
        /// Gets all states defined in this dictionary.
        /// </summary>
        /// <returns>All states in this directory.</returns>
        public IEnumerable<IState<TState, TEvent>> GetStates()
        {
            return new List<IState<TState, TEvent>>(this.dictionary.Values);
        }
    }
}