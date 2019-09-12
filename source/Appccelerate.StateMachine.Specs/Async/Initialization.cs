//-------------------------------------------------------------------------------
// <copyright file="Initialization.cs" company="Appccelerate">
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
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class Initialization
    {
        private const int TestState = 1;

        [Scenario]
        public void Start(
            AsyncPassiveStateMachine<int, int> machine,
            bool entryActionExecuted,
            CurrentStateExtension currentStateExtension)
        {
            "establish an initialized state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(TestState)
                        .ExecuteOnEntry(() => entryActionExecuted = true);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(TestState)
                    .Build()
                    .CreatePassiveStateMachine();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);
            });

            "when starting the state machine".x(() =>
                machine.Start());

            "should set current state of state machine to state to which it is initialized".x(() =>
                currentStateExtension.CurrentState.Should().Be(TestState));

            "should execute entry action of state to which state machine is initialized".x(() =>
                entryActionExecuted.Should().BeTrue());
        }
    }
}