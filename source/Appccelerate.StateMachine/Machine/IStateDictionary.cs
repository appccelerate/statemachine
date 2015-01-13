//-------------------------------------------------------------------------------
// <copyright file="IStateDictionary.cs" company="Appccelerate">
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

    /// <summary>
    /// Manages the states of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IStateDictionary<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Gets the <see cref="IState{TState,TEvent}"/> with the specified state id.
        /// </summary>
        /// <value>State with the specified id.</value>
        /// <param name="stateId">The State id.</param>
        /// <returns>The State with the specified id.</returns>
        IState<TState, TEvent> this[TState stateId]
        {
            get;
        }

        /// <summary>
        /// Gets all states defined in this dictionary.
        /// </summary>
        /// <returns>All states in this directory.</returns>
        IEnumerable<IState<TState, TEvent>> GetStates();
    }
}