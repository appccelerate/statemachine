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

namespace Appccelerate.StateMachine.Specs.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using Appccelerate.StateMachine.Machine.Building;
    using FakeItEasy;
    using FluentAssertions;
    using Machine;
    using Xbehave;

    public class Initialization
    {
        private const int TestState = 1;

        [Scenario]
        public void Start(
            PassiveStateMachine<int, int> machine,
            bool entryActionExecuted,
            CurrentStateExtension currentStateExtension,
            IExtension<int, int> extension)
        {
            "establish a state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(TestState)
                        .ExecuteOnEntry(() => entryActionExecuted = true);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(TestState)
                    .Build()
                    .CreatePassiveStateMachine();

                extension = A.Fake<IExtension<int, int>>();
                machine.AddExtension(extension);

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);
            });

            "when starting the state machine".x(() =>
                machine.Start());

            "it should set current state of state machine to state to which it is initialized".x(() =>
                currentStateExtension.CurrentState.Should().Be(TestState));

            "it should execute entry action of state to which state machine is initialized".x(() =>
                entryActionExecuted.Should().BeTrue());

            "it should notify extensions that it is entering the initial state".x(() =>
                A.CallTo(() => extension.EnteringInitialState(
                        A<IStateMachineInformation<int, int>>._,
                        TestState))
                    .MustHaveHappened());

            "it should notify extensions that it has entered the initial state".x(() =>
                A.CallTo(() => extension.EnteredInitialState(
                        An.StateMachineInformation<int, int>(machine),
                        TestState,
                        An.TransitionContext<int, int>()))
                    .MustHaveHappened());

            "it should notify extensions that the state machine is started".x(() =>
                A.CallTo(() => extension.StartedStateMachine(
                        A<IStateMachineInformation<int, int>>._))
                    .MustHaveHappened());
        }

        [Scenario]
        public void MissingInitialize(
            StateMachineDefinitionBuilder<int, int> stateMachineDefinitionBuilder,
            Action build)
        {
            "establish a state machine definition without initialize".x(() =>
            {
                stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(TestState);
            });

            "when building the state machine from the definition".x(() =>
                build = () => stateMachineDefinitionBuilder
                    .Build());

            "it should throw an InvalidOperationException".x(() =>
                build.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("Initial state is not configured."));
        }
    }
}