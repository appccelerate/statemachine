//-------------------------------------------------------------------------------
// <copyright file="StatesExceptionMessages.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.States
{
    using System.Globalization;

    /// <summary>
    /// Holds all exception messages
    /// </summary>
    public static class StatesExceptionMessages
    {
        /// <summary>
        /// State cannot be its own super-state..
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>error message</returns>
        public static string StateCannotBeItsOwnSuperState(string state)
        {
            return string.Format(CultureInfo.InvariantCulture, "State {0} cannot be its own super-state.", state);
        }

        /// <summary>
        /// State cannot be the initial sub-state to itself.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>error message</returns>
        public static string StateCannotBeTheInitialSubStateToItself(string state)
        {
            return string.Format(
                CultureInfo.InvariantCulture, "State {0} cannot be the initial sub-state to itself.", state);
        }

        /// <summary>
        /// State cannot be the initial state of super state because it is not a direct sub-state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="superState">State of the super.</param>
        /// <returns>error message</returns>
        public static string StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(
            string state, string superState)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "State {0} cannot be the initial state of super state {1} because it is not a direct sub-state.",
                state,
                superState);
        }
    }
}