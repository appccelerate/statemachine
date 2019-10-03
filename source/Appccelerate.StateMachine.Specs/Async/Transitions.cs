//-------------------------------------------------------------------------------
// <copyright file="Transitions.cs" company="Appccelerate">
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
    using System.Threading.Tasks;
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class Transitions
    {
        private const int SourceState = 1;
        private const int DestinationState = 2;
        private const int Event = 2;

        private const string Parameter = "parameter";

        private static readonly CurrentStateExtension CurrentStateExtension = new CurrentStateExtension();

        [Scenario]
        public void ExecutingTransition(
            AsyncPassiveStateMachine<int, int> machine,
            string actualParameter,
            string asyncActualParameter,
            bool exitActionExecuted,
            bool entryActionExecuted,
            bool asyncExitActionExecuted,
            bool asyncEntryActionExecuted)
        {
            "establish a state machine with transitions".x(async () =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                        .ExecuteOnExit(() => exitActionExecuted = true)
                        .ExecuteOnExit(async () =>
                            {
                                asyncExitActionExecuted = true;
                                await Task.Yield();
                            })
                        .On(Event).Goto(DestinationState)
                            .Execute((string p) => actualParameter = p)
                            .Execute(async (string p) =>
                                {
                                    asyncActualParameter = p;
                                    await Task.Yield();
                                });

                stateMachineDefinitionBuilder
                    .In(DestinationState)
                        .ExecuteOnEntry(() => entryActionExecuted = true)
                        .ExecuteOnEntry(async () =>
                        {
                            asyncEntryActionExecuted = true;
                            await Task.Yield();
                        });

                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.AddExtension(CurrentStateExtension);

                await machine.Start();
            });

            "when firing an event onto the state machine".x(()
                => machine.Fire(Event, Parameter));

            "it should execute transition by switching state".x(()
                => CurrentStateExtension.CurrentState.Should().Be(DestinationState));

            "it should execute synchronous transition actions".x(()
                => actualParameter.Should().NotBeNull());

            "it should execute asynchronous transition actions".x(()
                => asyncActualParameter.Should().NotBeNull());

            "it should pass parameters to transition action".x(()
                => actualParameter.Should().Be(Parameter));

            "it should execute synchronous exit action of source state".x(()
                => exitActionExecuted.Should().BeTrue());

            "it should execute asynchronous exit action of source state".x(()
                => asyncExitActionExecuted.Should().BeTrue());

            "it should execute synchronous entry action of destination state".x(()
                => entryActionExecuted.Should().BeTrue());

            "it should execute asynchronous entry action of destination state".x(()
                => asyncEntryActionExecuted.Should().BeTrue());
        }
    }
}