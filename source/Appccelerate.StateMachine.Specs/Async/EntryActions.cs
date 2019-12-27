//-------------------------------------------------------------------------------
// <copyright file="EntryActions.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class EntryActions
    {
        private const int State = 1;

        [Scenario]
        public void EntryAction(
            AsyncPassiveStateMachine<int, int> machine,
            bool entryActionExecuted,
            bool asyncEntryActionExecuted)
        {
            "establish a state machine with entry action on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnEntry(() => entryActionExecuted = true)
                        .ExecuteOnEntry(async () =>
                        {
                            asyncEntryActionExecuted = true;
                            await Task.Yield();
                        });

                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when entering the state".x(async () =>
                await machine.Start());

            "it should execute the synchronous entry action".x(()
                => entryActionExecuted.Should().BeTrue());

            "it should execute the asynchronous entry action".x(()
                => asyncEntryActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void EntryActionWithParameter(
            AsyncPassiveStateMachine<int, int> machine,
            string receivedParameter,
            string asyncReceivedParameter)
        {
            const string Parameter = "parameter";

            "establish a state machine with entry action with parameter on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnEntryParametrized(p => receivedParameter = p, Parameter)
                        .ExecuteOnEntryParametrized(
                            async p =>
                            {
                                asyncReceivedParameter = p;
                                await Task.Yield();
                            },
                            Parameter);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when entering the state".x(async () =>
                await machine.Start());

            "it should execute the entry synchronous action".x(()
                => receivedParameter.Should().NotBeNull());

            "it should pass parameter to the synchronous entry action".x(()
                => receivedParameter.Should().Be(Parameter));

            "it should execute the asynchronous entry action".x(()
                => asyncReceivedParameter.Should().NotBeNull());

            "it should pass parameter to the asynchronous entry action".x(()
                => asyncReceivedParameter.Should().Be(Parameter));
        }

        [Scenario]
        public void MultipleEntryActions(
            AsyncPassiveStateMachine<int, int> machine,
            bool entryAction1Executed,
            bool asyncEntryAction1Executed,
            bool entryAction2Executed,
            bool asyncEntryAction2Executed)
        {
            "establish a state machine with several entry actions on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnEntry(() => entryAction1Executed = true)
                        .ExecuteOnEntry(async () =>
                        {
                            asyncEntryAction1Executed = true;
                            await Task.Yield();
                        })
                        .ExecuteOnEntry(() => entryAction2Executed = true)
                        .ExecuteOnEntry(async () =>
                        {
                            asyncEntryAction2Executed = true;
                            await Task.Yield();
                        });
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when entering the state".x(async () =>
                await machine.Start());

            "it should execute all entry actions".x(()
                => new[]
                    {
                        entryAction1Executed,
                        asyncEntryAction1Executed,
                        entryAction2Executed,
                        asyncEntryAction2Executed
                    }.Should().Equal(true, true, true, true));
        }

        [Scenario]
        public void EventArgument(
            AsyncPassiveStateMachine<int, int> machine,
            int passedArgument,
            int asyncPassedArgument)
        {
            const int Event = 3;
            const int AnotherState = 3;
            const int Argument = 17;

            "establish a state machine with an entry action taking an event argument".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();

                stateMachineDefinitionBuilder
                    .In(State)
                        .On(Event).Goto(AnotherState);

                stateMachineDefinitionBuilder
                    .In(AnotherState)
                        .ExecuteOnEntry((int a) => passedArgument = a)
                        .ExecuteOnEntry(async (int a) =>
                        {
                            asyncPassedArgument = Argument;
                            await Task.Yield();
                        });

                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when entering the state".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Event, Argument);
            });

            "it should pass event argument to synchronous entry action".x(()
                => passedArgument.Should().Be(Argument));

            "it should pass event argument to asynchronous entry action".x(()
                => asyncPassedArgument.Should().Be(Argument));
        }
    }
}