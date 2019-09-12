//-------------------------------------------------------------------------------
// <copyright file="ExceptionMessages.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Globalization;
    using States;

    /// <summary>
    /// Holds all exception messages.
    /// </summary>
    public static class ExceptionMessages
    {
        public const string InitialStateNotConfigured = "Initial state is not configured.";

        /// <summary>
        /// Value is not initialized.
        /// </summary>
        public const string ValueNotInitialized = "Value is not initialized";

        /// <summary>
        /// State machine is already initialized.
        /// </summary>
        public const string StateMachineIsAlreadyInitialized = "state machine is already initialized";

        /// <summary>
        /// State machine has not yet entered initial state.
        /// </summary>
        public const string StateMachineHasNotYetEnteredInitialState = "Initial state is not yet entered.";

        /// <summary>
        /// There must not be more than one transition for a single event of a state with no guard.
        /// </summary>
        public const string OnlyOneTransitionMayHaveNoGuard = "There must not be more than one transition for a single event of a state with no guard.";

        /// <summary>
        /// Transition without guard has to be last declared one.
        /// </summary>
        public const string TransitionWithoutGuardHasToBeLast = "The transition without guard has to be the last defined transition because state machine checks transitions in order of declaration.";

        public const string CannotSetALastActiveStateThatIsNotASubState = "The state that is set as the last active state of a super state has to be a sub state";

        public static string CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet<TState, TEvent>(TState newSuperStateId, IStateDefinition<TState, TEvent> stateAlreadyHavingASuperState)
            where TState : IComparable
            where TEvent : IComparable
        {
            Guard.AgainstNullArgument("stateAlreadyHavingASuperState", stateAlreadyHavingASuperState);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Cannot set state {0} as a super state because the state {1} has already a super state {2}.",
                newSuperStateId,
                stateAlreadyHavingASuperState.Id,
                stateAlreadyHavingASuperState.SuperState.Id);
        }

        public static string CannotFindStateDefinition<TState>(TState state)
            where TState : IComparable
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Cannot find StateDefinition for state {0}. Are you sure you have configured this state via myStateDefinitionBuilder.In(..) or myStateDefinitionBuilder.DefineHierarchyOn(..)?",
                state);
        }
    }
}