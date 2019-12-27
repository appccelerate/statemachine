//-------------------------------------------------------------------------------
// <copyright file="BuildingExceptionMessages.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.Building
{
    using System.Globalization;

    /// <summary>
    /// Holds all exception messages.
    /// </summary>
    public static class BuildingExceptionMessages
    {
        public const string InitialStateNotConfigured = "Initial state is not configured.";

        public const string OnlyOneTransitionMayHaveNoGuard = "There must not be more than one transition for a single event of a state with no guard.";

        public const string TransitionWithoutGuardHasToBeLast = "The transition without guard has to be the last defined transition because state machine checks transitions in order of declaration.";

        public static string CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet<TState, TEvent>(
            TState newSuperStateId,
            BuildableStateDefinition<TState, TEvent> stateAlreadyHavingASuperState)
            where TState : notnull
            where TEvent : notnull
        {
            Guard.AgainstNullArgument("stateAlreadyHavingASuperState", stateAlreadyHavingASuperState);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Cannot set state {0} as a super state because the state {1} has already a super state {2}.",
                newSuperStateId,
                stateAlreadyHavingASuperState.Id,
                stateAlreadyHavingASuperState.SuperState!.Id);
        }

        public static string StateCannotBeItsOwnSuperState(string state)
        {
            return string.Format(CultureInfo.InvariantCulture, "State {0} cannot be its own super-state.", state);
        }

        public static string StateCannotBeTheInitialSubStateToItself(string state)
        {
            return string.Format(
                CultureInfo.InvariantCulture, "State {0} cannot be the initial sub-state to itself.", state);
        }

        public static string StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(
            string state, string superState)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "State {0} cannot be the initial state of super state {1} because it is not a direct sub-state.",
                state,
                superState);
        }

        public static string TransitionDoesAlreadyExist<TState, TEvent>(
            BuildableTransitionDefinition<TState, TEvent> transition,
            BuildableStateDefinition<TState, TEvent> state)
            where TState : notnull
            where TEvent : notnull
        {
            Guard.AgainstNullArgument("transition", transition);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Transition {0} cannot be added to the state {1} because it has already been added to the state {2}.",
                transition,
                state,
                transition.Source);
        }
    }
}