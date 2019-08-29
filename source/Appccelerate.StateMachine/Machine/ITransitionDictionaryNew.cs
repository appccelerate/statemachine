//-------------------------------------------------------------------------------
// <copyright file="ITransitionDictionary.cs" company="Appccelerate">
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
    using System.Collections.Generic;

    using Appccelerate.StateMachine.Machine.Transitions;

    public interface ITransitionDictionaryNew<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Adds the specified event id.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="transition">The transition.</param>
        void Add(TEvent eventId, TransitionNew<TState, TEvent> transition);

        /// <summary>
        /// Gets all transitions.
        /// </summary>
        /// <returns>All transitions.</returns>
        IEnumerable<TransitionInfoNew<TState, TEvent>> GetTransitions();

        /// <summary>
        /// Gets the transitions for the specified event id.
        /// </summary>
        /// <value>transitions for the event id.</value>
        /// <param name="eventId">Id of the event.</param>
        /// <returns>The transitions for the event id.</returns>
        ICollection<TransitionNew<TState, TEvent>> this[TEvent eventId] { get; }
    }
}