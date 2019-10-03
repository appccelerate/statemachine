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
    using FluentAssertions;
    using Infrastructure;
    using Machine;
    using Specs;
    using Xbehave;

    public class Initialization
    {
        private const int TestState = 1;

        [Scenario]
        public void Start(
            PassiveStateMachine<int, int> machine,
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
                    .Build()
                    .CreatePassiveStateMachine();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                machine.Initialize(TestState);
            });

            "when starting the state machine".x(() =>
                machine.Start());

            "should set current state of state machine to state to which it is initialized".x(() =>
                currentStateExtension.CurrentState.Should().Be(TestState));

            "should execute entry action of state to which state machine is initialized".x(() =>
                entryActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void Initialize(
            PassiveStateMachine<int, int> machine,
            bool entryActionExecuted)
        {
            "establish a state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(TestState)
                    .ExecuteOnEntry(() => entryActionExecuted = true);
                machine = stateMachineDefinitionBuilder
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when state machine is initialized".x(() =>
                machine.Initialize(TestState));

            "should not yet execute any entry actions".x(() =>
                entryActionExecuted.Should().BeFalse());
        }

        [Scenario]
        public void Reinitialization(
            PassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish an initialized state machine".x(() =>
            {
                machine = new StateMachineDefinitionBuilder<int, int>()
                    .Build()
                    .CreatePassiveStateMachine();
                machine.Initialize(TestState);
            });

            "when state machine is initialized again".x(() =>
                receivedException = Catch.Exception(() =>
                    machine.Initialize(TestState)));

            "should throw an invalid operation exception".x(() =>
                receivedException
                    .Should().BeAssignableTo<InvalidOperationException>()
                    .Which.Message
                    .Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized));
        }

        [Scenario]
        public void StartingAnUninitializedStateMachine(
            PassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish an uninitialized state machine".x(() =>
                machine = new StateMachineDefinitionBuilder<int, int>()
                    .Build()
                    .CreatePassiveStateMachine());

            "when starting the state machine".x(() =>
                receivedException = Catch.Exception(() =>
                    machine.Start()));

            "should throw an invalid operation exception".x(() =>
                receivedException
                    .Should().BeAssignableTo<InvalidOperationException>()
                    .Which.Message
                    .Should().Be(ExceptionMessages.StateMachineNotInitialized));
        }

        [Scenario]
        public void InitializeALoadedStateMachine(
            PassiveStateMachine<int, int> machine,
            Exception receivedException,
            CurrentStateExtension currentStateExtension)
        {
            "establish a loaded initialized state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(1);
                machine = stateMachineDefinitionBuilder
                    .Build()
                    .CreatePassiveStateMachine();

                var loader = new Persisting.StateMachineLoader<int>();
                loader.SetCurrentState(new Initializable<int> { Value = 1 });
                loader.SetHistoryStates(new Dictionary<int, int>());
                machine.Load(loader);
            });

            "when initializing the state machine".x(() =>
                receivedException = Catch.Exception(() =>
                    machine.Initialize(0)));

            "should throw an invalid operation exception".x(() =>
                receivedException
                    .Should().BeAssignableTo<InvalidOperationException>()
                    .Which.Message
                    .Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized));
        }
    }
}