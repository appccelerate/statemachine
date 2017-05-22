//-------------------------------------------------------------------------------
// <copyright file="ITransitionResult.cs" company="Appccelerate">
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

    /// <summary>
    /// Represents the result of a transition.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface ITransitionResult<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="ITransitionResult{TState, TEvent}"/> is fired.
        /// </summary>
        /// <value><c>true</c> if fired; otherwise, <c>false</c>.</value>
        bool Fired { get; }

        /// <summary>
        /// Gets the new state.
        /// </summary>
        /// <value>The new state.</value>
        IState<TState, TEvent> NewState { get; }
    }
}