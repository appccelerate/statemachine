//-------------------------------------------------------------------------------
// <copyright file="IAsyncStateMachineSaver.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure;

    public interface IAsyncStateMachineSaver<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Saves the current state of the state machine.
        /// </summary>
        /// <param name="currentStateId">Id of the current state.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveCurrentState(IInitializable<TState> currentStateId);

        /// <summary>
        /// Saves the last active states of all super states.
        /// </summary>
        /// <param name="historyStates">Key = id of the super state; Value = if of last active state of super state.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveHistoryStates(IReadOnlyDictionary<TState, TState> historyStates);

        /// <summary>
        /// Saves the not yet processed events of the state machine.
        /// </summary>
        /// <param name="events">The not yet processed events.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveEvents(IReadOnlyCollection<EventInformation<TEvent>> events);

        /// <summary>
        /// Saves the not yet processed priority events of the state machine.
        /// </summary>
        /// <param name="priorityEvents">The not yet processed priority events.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SavePriorityEvents(IReadOnlyCollection<EventInformation<TEvent>> priorityEvents);
    }
}