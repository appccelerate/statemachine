//-------------------------------------------------------------------------------
// <copyright file="StateMachineAssertionsExtensionMethods.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine
{
    using System;
    using Appccelerate.StateMachine.Machine.Transitions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;
    using StateMachine.Machine;
    using StateMachine.Machine.States;

    public static class StateMachineAssertionsExtensionMethods
    {
        public static void BeSuccessfulTransitionResultWithNewState<TStates, TEvents>(this ObjectAssertions assertions, IStateDefinition<TStates, TEvents> expectedNewState)
            where TStates : IComparable
            where TEvents : IComparable
        {
            var transitionResult = assertions.Subject;

            Execute.Assertion
                .ForCondition(assertions.Subject is FiredTransitionResult<TStates>)
                .FailWith("expected successful (fired) transition result.");

            if (assertions.Subject is FiredTransitionResult<TStates> fired)
            {
                Execute.Assertion
                    .ForCondition(fired.NewState.CompareTo(expectedNewState.Id) == 0)
                    .FailWith("expected transition result with new state = `" + expectedNewState.Id + "`, but found `" + fired.NewState + "`.");
            }
        }

        public static void BeNotFiredTransitionResult<TStates>(this ObjectAssertions assertions)
            where TStates : IComparable
        {
            var transitionResult = (ITransitionResult<TStates>)assertions.Subject;

            Execute.Assertion
                   .ForCondition(!transitionResult.Fired)
                   .FailWith("expected not fired transition result.");
        }
    }
}