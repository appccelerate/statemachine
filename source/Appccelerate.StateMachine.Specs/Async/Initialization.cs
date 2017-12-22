//-------------------------------------------------------------------------------
// <copyright file="Initialization.cs" company="Appccelerate">
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
    using System;
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Infrastructure;
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
            AsyncPassiveStateMachine<int, int> machine,
            bool entryActionExecuted)
        {
            "establish an initialized state machine"._(() =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();

                    machine.AddExtension(this.testExtension);

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
            AsyncPassiveStateMachine<int, int> machine,
            bool entryActionExecuted)
        {
            "establish a state machine"._(() =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();

                    machine.AddExtension(this.testExtension);

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
            AsyncPassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish an initialized state machine"._(() =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();
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
            AsyncPassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish an uninitialized state machine"._(() =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();
                });

            "when starting the state machine"._(async () =>
                receivedException = await Catch.Exception(async () => await machine.Start()));

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
            AsyncPassiveStateMachine<int, int> machine,
            Exception receivedException)
        {
            "establish a loaded initialized state machine"._(async () =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();

                    var loader = new Persisting.StateMachineLoader<int>();

                    loader.SetCurrentState(new Initializable<int> { Value = 1 });
                    loader.SetHistoryStates(new Dictionary<int, int>());

                    await machine.Load(loader);                });

            "when initializing the state machine"._(async () =>
                    receivedException = await Catch.Exception(async () =>
                        await machine.Initialize(0)));

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