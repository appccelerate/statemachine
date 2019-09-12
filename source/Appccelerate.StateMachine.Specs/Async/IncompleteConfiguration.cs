//-------------------------------------------------------------------------------
// <copyright file="IncompleteConfiguration.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Async
{
    using System;
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class IncompleteConfiguration
    {
        [Scenario]
        public void TransitionActionException(
            AsyncPassiveStateMachine<int, int> machine,
            Exception receivedException,
            int state = 0)
        {
            "establish a StateDefinition without configurations".x(() =>
            {
                machine = new StateMachineDefinitionBuilder<int, int>()
                    .WithInitialState(state)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when the state machine is started".x(async () =>
                receivedException = await Catch.Exception(async () => await machine.Start()));

            "it should throw an exception, indicating the missing configuration".x(() =>
                receivedException
                    .Message
                    .Should()
                    .Be($"Cannot find StateDefinition for state {state}. Are you sure you have configured this state via myStateDefinitionBuilder.In(..) or myStateDefinitionBuilder.DefineHierarchyOn(..)?"));
        }

        [Scenario]
        public void BuildingAStateMachineWithoutInitialStateThenInvalidOperationException()
        {
            var testee = new StateMachineDefinitionBuilder<int, int>();

            Action action = () => testee.Build();

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Initial state is not configured.");
        }
    }
}