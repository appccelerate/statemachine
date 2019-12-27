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

namespace Appccelerate.StateMachine.Specs.Sync
{
    using FluentAssertions;
    using Machine;
    using Xbehave;

    public class ExitActions
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;

        [Scenario]
        public void ExitAction(
            PassiveStateMachine<int, int> machine,
            bool exitActionExecuted)
        {
            "establish a state machine with exit action on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExit(() => exitActionExecuted = true)
                        .On(Event).Goto(AnotherState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(() =>
            {
                machine.Start();
                machine.Fire(Event);
            });

            "it should execute the exit action".x(() =>
                exitActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void ExitActionWithParameter(
            PassiveStateMachine<int, int> machine,
            string parameter)
        {
            const string Parameter = "parameter";

            "establish a state machine with exit action with parameter on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExitParametrized(p => parameter = p, Parameter)
                        .On(Event).Goto(AnotherState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(() =>
            {
                machine.Start();
                machine.Fire(Event);
            });

            "it should execute the exit action".x(() =>
                parameter.Should().NotBeNull());

            "it should pass parameter to the exit action".x(() =>
                parameter.Should().Be(Parameter));
        }

        [Scenario]
        public void MultipleExitActions(
            PassiveStateMachine<int, int> machine,
            bool exitAction1Executed,
            bool exitAction2Executed)
        {
            "establish a state machine with several exit actions on a state".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExit(() => exitAction1Executed = true)
                        .ExecuteOnExit(() => exitAction2Executed = true)
                        .On(Event).Goto(AnotherState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(() =>
            {
                machine.Start();
                machine.Fire(Event);
            });

            "It should execute all exit actions".x(() =>
            {
                exitAction1Executed
                    .Should().BeTrue("first action should be executed");

                exitAction2Executed
                    .Should().BeTrue("second action should be executed");
            });
        }

        [Scenario]
        public void EventArgument(
            PassiveStateMachine<int, int> machine,
            int passedArgument)
        {
            const int Argument = 17;

            "establish a state machine with an exit action taking an event argument".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(State)
                        .ExecuteOnExit((int argument) => passedArgument = argument)
                        .On(Event).Goto(AnotherState);
                stateMachineDefinitionBuilder
                    .In(AnotherState)
                        .ExecuteOnEntry((int argument) => passedArgument = argument);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(State)
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when leaving the state".x(() =>
            {
                machine.Start();
                machine.Fire(Event, Argument);
            });

            "it should pass event argument to exit action".x(() =>
                passedArgument.Should().Be(Argument));
        }
    }
}