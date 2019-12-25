// <copyright file="An.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Sync
{
    using System;
    using Appccelerate.StateMachine.Machine;
    using FakeItEasy;

    public static class An
    {
        public static IStateMachineInformation<TState, TEvent> StateMachineInformation<TState, TEvent>(IStateMachine<int, int> machine)
            where TState : IComparable
            where TEvent : IComparable
        {
            return A<IStateMachineInformation<TState, TEvent>>
                .That
                .Matches(x =>
                    x.Name == machine.GetType().FullNameToString());
        }

        public static ITransitionContext<TState, TEvent> TransitionContext<TState, TEvent>()
            where TState : IComparable
            where TEvent : IComparable
        {
            return A<ITransitionContext<TState, TEvent>>._;
        }
    }
}