//-------------------------------------------------------------------------------
// <copyright file="Initialization.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

namespace Appccelerate.StateMachine
{
    using System;

    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Persistence;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;

    public class Initialization
    {
        private const int TestState = 1;

        private readonly CurrentStateExtension testExtension = new CurrentStateExtension();

        [Scenario]
        public void Start(
            PassiveStateMachine<int, int> machine,
            bool entryActionExecuted)
        {
            "establish an initialized state machine"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();

                    machine.AddExtension(testExtension);

                    machine.In(TestState)
                        .ExecuteOnEntry(() => entryActionExecuted = true);

                    machine.Initialize(TestState);
                });

            "when starting the state machine"._(() => 
                machine.Start());

            "should set current state of state machine to state to which it is initialized"._(() =>
                this.testExtension.CurrentState.Should().Be(TestState));

            "should execute entry action of state to which state machine is initialized"._(() =>
                entryActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void Initialize(
            PassiveStateMachine<int, int> machine,
            bool entryActionExecuted)
        {
            "establish a state machine"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();

                    machine.AddExtension(testExtension);

                    machine.In(TestState)
                        .ExecuteOnEntry(() => entryActionExecuted = true);
                });

            "when state machine is initialized"._(() =>
                machine.Initialize(TestState));

            "should not yet execute any entry actions"._(() =>
                entryActionExecuted.Should().BeFalse());
        }

        [Scenario]
        public void Reinitialization(
            PassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish an initialized state machine"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();
                    machine.Initialize(TestState);
                });

            "when state machine is initialized again"._(() =>
                {
                    try
                    {
                        machine.Initialize(TestState);
                    }
                    catch (Exception e)
                    {
                        receivedException = e;
                    }
                });

            "should throw an invalid operation exception"._(() =>
                {
                    receivedException
                        .Should().BeAssignableTo<InvalidOperationException>();
                    receivedException.Message
                        .Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
                });
        }

        [Scenario]
        public void StartingAnUninitializedStateMachine(
            PassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish an uninitialized state machine"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();
                });

            "when starting the state machine"._(() =>
                receivedException = Catch.Exception(() => 
                    machine.Start()));

            "should throw an invalid operation exception"._(() =>
                {
                    receivedException
                        .Should().BeAssignableTo<InvalidOperationException>();
                    receivedException.Message
                        .Should().Be(ExceptionMessages.StateMachineNotInitialized);
                });
        }

        [Scenario]
        public void InitializeALoadedStateMachine(
            PassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish a loaded state machine"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();

                    machine.Load(A.Fake<IStateMachineLoader<int>>());
                });

            "when initializing the state machine"._(() =>
                    receivedException = Catch.Exception(() => 
                        machine.Initialize(0)));

            "should throw an invalid operation exception"._(() =>
                {
                    receivedException
                        .Should().BeAssignableTo<InvalidOperationException>();
                    receivedException.Message
                        .Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
                });
        }
    }
}