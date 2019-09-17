//-------------------------------------------------------------------------------
// <copyright file="IAsyncStateMachineLoader.cs" company="Appccelerate">
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

    public interface IAsyncStateMachineLoader<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Returns the state to be set as the current state of the state machine.
        /// </summary>
        /// <returns>State id.</returns>
        Task<IInitializable<TState>> LoadCurrentState();

        /// <summary>
        /// Returns the last active state of all super states that have a last active state (i.e. they count as visited).
        /// </summary>
        /// <returns>Key = id of super state, Value = id of last active state.</returns>
        Task<IReadOnlyDictionary<TState, TState>> LoadHistoryStates();

        /// <summary>
        /// Returns the events to be processed by the state machine.
        /// </summary>
        /// <returns>The events.</returns>
        Task<IReadOnlyCollection<EventInformation<TEvent>>> LoadEvents();

        /// <summary>
        /// Returns the priority events to be processed by the state machine.
        /// </summary>
        /// <returns>The priority events.</returns>
        Task<IReadOnlyCollection<EventInformation<TEvent>>> LoadPriorityEvents();
    }
}