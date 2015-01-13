//-------------------------------------------------------------------------------
// <copyright file="TransitionsExceptionMessages.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;
    using System.Globalization;
    
    /// <summary>
    /// Holds all exception messages
    /// </summary>
    public static class TransitionsExceptionMessages
    {
        /// <summary>
        /// Transition cannot be added to the state because it has already been added to the state.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="transition">The transition.</param>
        /// <param name="state">The state.</param>
        /// <returns>error message</returns>
        public static string TransitionDoesAlreadyExist<TState, TEvent>(ITransition<TState, TEvent> transition, IState<TState, TEvent> state)
            where TState : IComparable
            where TEvent : IComparable
        {
            Ensure.ArgumentNotNull(transition, "transition");

            return string.Format(
                        CultureInfo.InvariantCulture,
                        "Transition {0} cannot be added to the state {1} because it has already been added to the state {2}.",
                        transition,
                        state,
                        transition.Source);
        }
    }
}