//-------------------------------------------------------------------------------
// <copyright file="Transitions.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Async
{
    using System.Threading.Tasks;
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
            "establish a state machine with transitions"._(async () =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();

                    machine.AddExtension(CurrentStateExtension);

                    machine.In(SourceState)
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

                    machine.In(DestinationState)
                        .ExecuteOnEntry(() => entryActionExecuted = true)
                        .ExecuteOnEntry(async () =>
                        {
                            asyncEntryActionExecuted = true;
                            await Task.Yield();
                        });

                    machine.Initialize(SourceState);
                    await machine.Start();
                });

            "when firing an event onto the state machine"._(()
                => machine.Fire(Event, Parameter));

            "it should execute transition by switching state"._(()
                => CurrentStateExtension.CurrentState.Should().Be(DestinationState));

            "it should execute synchronous transition actions"._(()
                => actualParameter.Should().NotBeNull());

            "it should execute asynchronous transition actions"._(()
                => asyncActualParameter.Should().NotBeNull());

            "it should pass parameters to transition action"._(()
                => actualParameter.Should().Be(Parameter));

            "it should execute synchronous exit action of source state"._(()
                => exitActionExecuted.Should().BeTrue());

            "it should execute asynchronous exit action of source state"._(()
                => asyncExitActionExecuted.Should().BeTrue());

            "it should execute synchronous entry action of destination state"._(()
                => entryActionExecuted.Should().BeTrue());

            "it should execute asynchronous entry action of destination state"._(()
                => asyncEntryActionExecuted.Should().BeTrue());
        }
    }
}