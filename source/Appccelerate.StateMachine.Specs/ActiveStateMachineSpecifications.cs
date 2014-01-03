//-------------------------------------------------------------------------------
// <copyright file="ActiveStateMachineSpecifications.cs" company="Appccelerate">
//   Copyright (c) 2008-2013
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
    using System.Threading;

    using Appccelerate.StateMachine.Machine;

    using FakeItEasy;

    using FluentAssertions;

    using global::Machine.Specifications;

    [Subject("Active state machine creation")]
    public class When_instantiating_an_active_state_machine_without_constructor_arguments
    {
        private static ActiveStateMachine<string, int> machine;
        private static StateMachineNameReporter reporter;

        Establish context = () =>
            {
                machine = new ActiveStateMachine<string, int>();

                reporter = new StateMachineNameReporter();
            };

        Because of = () =>
            machine.Report(reporter);

        It should_use_default_name_for_state_machine = () =>
            reporter.StateMachineName
                .Should().Be("Appccelerate.StateMachine.ActiveStateMachine<System.String,System.Int32>");
    }

    [Subject("Active state machine creation")]
    public class When_instantiating_an_active_state_machine_with_custom_name
    {
        const string Name = "custom name";

        private static ActiveStateMachine<string, int> machine;
        private static StateMachineNameReporter reporter;

        Establish context = () =>
        {
            machine = new ActiveStateMachine<string, int>(Name);

            reporter = new StateMachineNameReporter();
        };

        Because of = () =>
            machine.Report(reporter);

        It should_use_custom_name_for_state_machine = () =>
            reporter.StateMachineName
                .Should().Be(Name);
    }

    [Subject("Active state machine creation")]
    public class When_instantiating_an_active_state_machine_with_custom_factory
    {
        const string Name = "custom name";

        static ActiveStateMachine<string, int> machine;
        static StandardFactory<string, int> factory;

        Establish context = () =>
            {
                factory = A.Fake<StandardFactory<string, int>>();
            };

        Because of = () =>
            {
                machine = new ActiveStateMachine<string, int>("_", factory);

                machine.In("initial").On(42).Goto("answer");
            };

        It should_use_custom_factory_to_create_internal_instances = () =>
            A.CallTo(factory).MustHaveHappened();
    }

    [Subject("Active state machine")]
    public class When_firing_an_event_onto_an_initialized_and_started_active_state_machine
    {
        const int FirstEvent = 0;
        const int SecondEvent = 1;

        static IStateMachine<string, int> machine;
        static AutoResetEvent signal;

        Establish context = () =>
        {
            signal = new AutoResetEvent(false);

            machine = new ActiveStateMachine<string, int>();

            machine.In("A").On(FirstEvent).Goto("B");
            machine.In("B").On(SecondEvent).Goto("C");
            machine.In("C").ExecuteOnEntry(() => signal.Set());

            machine.Initialize("A");
        };

        Because of = () =>
        {
            machine.Fire(FirstEvent);
            machine.Fire(SecondEvent);
            machine.Start();
        };

        It should_queue_event_at_the_end = () =>
            signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state");
    }

    [Subject("Active state machine")]
    public class When_firing_a_priority_event_onto_an_initialized_and_started_active_state_machine
    {
        const int FirstEvent = 0;
        const int SecondEvent = 1;

        static IStateMachine<string, int> machine;
        static AutoResetEvent signal;

        Establish context = () =>
        {
            signal = new AutoResetEvent(false);

            machine = new ActiveStateMachine<string, int>();

            machine.In("A").On(SecondEvent).Goto("B");
            machine.In("B").On(FirstEvent).Goto("C");
            machine.In("C").ExecuteOnEntry(() => signal.Set());

            machine.Initialize("A");
        };

        Because of = () =>
        {
            machine.Fire(FirstEvent);
            machine.FirePriority(SecondEvent);
            machine.Start();
        };

        It should_queue_event_at_the_end = () =>
            signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state");
    }

    [Subject("Active state machine")]
    public class When_firing_an_event_with_parameter_onto_an_initialized_and_started_active_state_machine
    {
        const int FirstEvent = 0;
        const int SecondEvent = 1;

        static IStateMachine<string, int> machine;
        static AutoResetEvent signal;

        Establish context = () =>
        {
            signal = new AutoResetEvent(false);

            machine = new ActiveStateMachine<string, int>();

            machine.In("A").On(FirstEvent).Goto("B");
            machine.In("B").On(SecondEvent).Goto("C");
            machine.In("C").ExecuteOnEntry((string argument) => signal.Set());

            machine.Initialize("A");
        };

        Because of = () =>
        {
            machine.Fire(FirstEvent);
            machine.Fire(SecondEvent, "argument");
            machine.Start();
        };

        It should_queue_event_at_the_end = () =>
            signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state");
    }

    [Subject("Active state machine")]
    public class When_firing_a_priority_event_with_argument_onto_an_initialized_and_started_active_state_machine
    {
        const int FirstEvent = 0;
        const int SecondEvent = 1;

        static IStateMachine<string, int> machine;
        static AutoResetEvent signal;

        Establish context = () =>
        {
            signal = new AutoResetEvent(false);

            machine = new ActiveStateMachine<string, int>();

            machine.In("A").On(SecondEvent).Goto("B");
            machine.In("B").On(FirstEvent).Goto("C");
            machine.In("C").ExecuteOnEntry(() => signal.Set());

            machine.Initialize("A");
        };

        Because of = () =>
        {
            machine.Fire(FirstEvent);
            machine.FirePriority(SecondEvent, "argument");
            machine.Start();
        };

        It should_queue_event_at_the_end = () =>
            signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state");
    }
}