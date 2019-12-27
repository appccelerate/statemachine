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
    using System.Globalization;

    public static class ExceptionMessages
    {
        public const string StateMachineIsAlreadyInitialized = "state machine is already initialized";

        public const string StateMachineHasNotYetEnteredInitialState = "Initial state is not yet entered.";

        public const string CannotSetALastActiveStateThatIsNotASubState = "The state that is set as the last active state of a super state has to be a sub state";

        public static string CannotFindStateDefinition<TState>(
            TState state)
            where TState : notnull
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Cannot find StateDefinition for state {0}. Are you sure you have configured this state via myStateDefinitionBuilder.In(..) or myStateDefinitionBuilder.DefineHierarchyOn(..)?",
                state);
        }
    }
}