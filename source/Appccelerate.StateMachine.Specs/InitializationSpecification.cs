//-------------------------------------------------------------------------------
// <copyright file="InitializationSpecification.cs" company="Appccelerate">
//   Copyright (c) 2008-2014
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
    using global::Machine.Specifications;

    [Subject(Concern.Initialization)]
    public class When_an_initialized_state_machine_is_started
    {
        private const int TestState = 1;

        static PassiveStateMachine<int, int> machine;
        static bool entryActionExecuted;
        static CurrentStateExtension testExtension;

        Establish context = () =>
            {
                testExtension = new CurrentStateExtension();

                machine = new PassiveStateMachine<int, int>();

                machine.AddExtension(testExtension);

                machine.In(TestState)
                    .ExecuteOnEntry(() => entryActionExecuted = true);
            };

        Because of = () =>
            {
                machine.Initialize(TestState);
                machine.Start();
            };

        It should_set_current_state_of_state_machine_to_state_to_which_it_is_initialized = () => 
            testExtension.CurrentState.Should().Be(TestState);

        It should_execute_entry_action_of_state_to_which_state_machine_is_initialized = () => 
            entryActionExecuted.Should().BeTrue();
    }

    [Subject(Concern.Initialization)]
    public class When_an_state_machine_is_initialized
    {
        private const int TestState = 1;

        static PassiveStateMachine<int, int> machine;
        static bool entryActionExecuted;
        static CurrentStateExtension testExtension;

        Establish context = () =>
            {
                testExtension = new CurrentStateExtension();

                machine = new PassiveStateMachine<int, int>();

                machine.AddExtension(testExtension);

                machine.In(TestState)
                    .ExecuteOnEntry(() => entryActionExecuted = true);
            };

        Because of = () => 
            machine.Initialize(TestState);

        It should_not_yet_execute_any_entry_actions = () => 
            entryActionExecuted.Should().BeFalse();
    }

    [Subject(Concern.Initialization)]
    public class When_an_already_initialized_state_machine_is_initialized
    {
        private const int TestState = 1;

        static PassiveStateMachine<int, int> machine;
        static Exception receivedException;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
            machine.Initialize(TestState);
        };

        Because of = () =>
        {
            try
            {
                machine.Initialize(TestState);
            }
            catch (Exception e)
            {
                receivedException = e;
            }
        };

        It should_throw_an_invalid_operation_exception = () =>
        {
            receivedException
                .Should().BeAssignableTo<InvalidOperationException>();
            receivedException.Message
                .Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
        };
    }

    [Subject(Concern.Initialization)]
    public class When_an_uninitialized_state_machine_is_started
    {
        private const int TestState = 1;

        static PassiveStateMachine<int, int> machine;
        static Exception receivedException;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            try
            {
                machine.Start();
            }
            catch (Exception e)
            {
                receivedException = e;
            }
        };

        It should_throw_an_invalid_operation_exception = () =>
        {
            receivedException
                .Should().BeAssignableTo<InvalidOperationException>();
            receivedException.Message
                .Should().Be(ExceptionMessages.StateMachineNotInitialized);
        };
    }

    [Subject(Concern.Initialization)]
    public class When_a_loaded_state_machine_is_initialized
    {
        private const int TestState = 1;

        static IStateMachine<int, int> machine;
        static IStateMachineLoader<int> loader;
        static Exception receivedException;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();

            loader = A.Fake<IStateMachineLoader<int>>();
            machine.Load(loader);
        };

        Because of = () =>
        {
            receivedException = Catch.Exception(() => machine.Initialize(0));
        };

        It should_throw_an_invalid_operation_exception = () =>
        {
            receivedException
                .Should().BeAssignableTo<InvalidOperationException>();
            receivedException.Message
                .Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
        };
    }
}