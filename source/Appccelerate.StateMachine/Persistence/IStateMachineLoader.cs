//-------------------------------------------------------------------------------
// <copyright file="IStateMachineLoader.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Persistence
{
    using System;
    using System.Collections.Generic;

    using Appccelerate.StateMachine.Machine;

    public interface IStateMachineLoader<TState>
        where TState : IComparable
    {
        /// <summary>
        /// Returns the state to be set as the current state of the state machine.
        /// </summary>
        /// <returns>State id.</returns>
        Initializable<TState> LoadCurrentState();

        /// <summary>
        /// Returns the last active state of all super states that have a last active state (i.e. they count as visited).
        /// </summary>
        /// <returns>Key = id of super state, Value = id of last active state.</returns>
        IDictionary<TState, TState> LoadHistoryStates();
    }
}