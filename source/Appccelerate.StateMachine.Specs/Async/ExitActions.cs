// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExitActions.cs" company="Appccelerate">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.Specs.Async
{
    using System.Threading.Tasks;
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class ExitActions
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;

        [Scenario]
        public void ExitAction(
            AsyncPassiveStateMachine<int, int> machine,
            bool exitActionExecuted,
            bool asyncExitActionExecuted)
        {
            "establish a state machine with exit action on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExit(() => exitActionExecuted = true)
                        .ExecuteOnExit(async () =>
                        {
                            asyncExitActionExecuted = true;
                            await Task.Yield();
                        })
                        .On(Event).Goto(AnotherState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Event);
            });

            "it should execute the synchronous exit action".x(()
                => exitActionExecuted.Should().BeTrue());

            "it should execute the asynchronous exit action".x(()
                => asyncExitActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void ExitActionWithParameter(
            AsyncPassiveStateMachine<int, int> machine,
            string receivedParameter,
            string asyncReceivedParameter)
        {
            const string Parameter = "parameter";

            "establish a state machine with exit action with parameter on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExitParametrized(p => receivedParameter = p, Parameter)
                        .ExecuteOnExitParametrized(
                            async p =>
                            {
                                asyncReceivedParameter = p;
                                await Task.Yield();
                            },
                            Parameter)
                        .On(Event).Goto(AnotherState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Event);
            });

            "it should execute the synchronous exit action".x(() =>
                receivedParameter.Should().NotBeNull());

            "it should pass parameter to the synchronous exit action".x(() =>
                receivedParameter.Should().Be(Parameter));

            "it should execute the asynchronous exit action".x(() =>
                asyncReceivedParameter.Should().NotBeNull());

            "it should pass parameter to the asynchronous exit action".x(() =>
                asyncReceivedParameter.Should().Be(Parameter));
        }

        [Scenario]
        public void MultipleExitActions(
            AsyncPassiveStateMachine<int, int> machine,
            bool exitAction1Executed,
            bool exitAction2Executed,
            bool asyncExitAction1Executed,
            bool asyncExitAction2Executed)
        {
            "establish a state machine with several exit actions on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExit(() => exitAction1Executed = true)
                        .ExecuteOnExit(() => exitAction2Executed = true)
                        .ExecuteOnExit(async () =>
                        {
                            asyncExitAction1Executed = true;
                            await Task.Yield();
                        })
                        .ExecuteOnExit(async () =>
                        {
                            asyncExitAction2Executed = true;
                            await Task.Yield();
                        })
                        .On(Event).Goto(AnotherState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Event);
            });

            "it should execute all exit actions".x(()
                => new[]
                {
                    exitAction1Executed,
                    exitAction2Executed,
                    asyncExitAction1Executed,
                    asyncExitAction2Executed
                }.Should().Equal(true, true, true, true));
        }

        [Scenario]
        public void EventArgument(
            AsyncPassiveStateMachine<int, int> machine,
            int passedArgument)
        {
            const int Argument = 17;

            "establish a state machine with an exit action taking an event argument".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExit((int a) => passedArgument = a)
                        .On(Event).Goto(AnotherState);

                stateMachineDefinitionBuilder
                    .In(AnotherState)
                        .ExecuteOnEntry((int a) => passedArgument = a);

                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Event, Argument);
            });

            "it should pass event argument to exit action".x(() =>
                passedArgument.Should().Be(Argument));
        }
    }
}