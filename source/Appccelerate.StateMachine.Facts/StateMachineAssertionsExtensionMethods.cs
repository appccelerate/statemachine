//-------------------------------------------------------------------------------
// <copyright file="StateMachineAssertionsExtensionMethods.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine
{
    using System;
    using Appccelerate.StateMachine.Machine;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;

    public static class StateMachineAssertionsExtensionMethods
    {
        public static void BeSuccessfulTransitionResultWithNewState<TStates, TEvents>(this ObjectAssertions assertions, IState<TStates, TEvents> expectedNewState)
            where TStates : IComparable
            where TEvents : IComparable
        {
            ITransitionResult<TStates, TEvents> transitionResult = (ITransitionResult<TStates, TEvents>)assertions.Subject;

            Execute.Assertion
                   .ForCondition(transitionResult.Fired)
                   .FailWith("expected successful (fired) transition result.");

            Execute.Assertion
                   .ForCondition(transitionResult.NewState.Id.CompareTo(expectedNewState.Id) == 0)
                   .FailWith("expected transition result with new state = `" + expectedNewState.Id + "`, but found `" + transitionResult.NewState.Id + "`.");
        }

        public static void BeNotFiredTransitionResult<TStates, TEvents>(this ObjectAssertions assertions)
            where TStates : IComparable
            where TEvents : IComparable
        {
            ITransitionResult<TStates, TEvents> transitionResult = (ITransitionResult<TStates, TEvents>)assertions.Subject;

            Execute.Assertion
                   .ForCondition(!transitionResult.Fired)
                   .FailWith("expected not fired transition result.");
        }
    }
}