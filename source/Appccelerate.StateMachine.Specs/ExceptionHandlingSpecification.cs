//-------------------------------------------------------------------------------
// <copyright file="ExceptionHandlingSpecification.cs" company="Appccelerate">
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

    using Appccelerate.StateMachine.Machine.Events;

    using FluentAssertions;
    
    using global::Machine.Specifications;

    [Subject(Concern.ExceptionHandling)]
    public class When_a_transition_action_throws_an_exception : ExceptionSpecification
    {
        Establish context = () =>
            {
                machine.In(Values.Source)
                    .On(Values.Event).Goto(Values.Destination).Execute(() => { throw Values.Exception; });
            };

        Because of = () =>
            {
                machine.Initialize(Values.Source);
                machine.Start();
                machine.Fire(Values.Event, Values.Parameter);
            };
        
        Behaves_like<TransitionExceptionBehaviors> transition_exception_catcher;
    }

    [Subject(Concern.ExceptionHandling)]
    public class When_an_entry_action_in_a_transition_throws_an_exception : ExceptionSpecification
    {
        Establish context = () =>
            {
                machine.In(Values.Source)
                    .On(Values.Event).Goto(Values.Destination);

                machine.In(Values.Destination)
                    .ExecuteOnEntry(() => { throw Values.Exception; });
            };

        Because of = () =>
            {
                machine.Initialize(Values.Source);
                machine.Start();
                machine.Fire(Values.Event, Values.Parameter);
            };

        Behaves_like<TransitionExceptionBehaviors> transition_exception_catcher;
    }

    [Subject(Concern.ExceptionHandling)]
    public class When_an_exit_action_in_a_transition_throws_an_exception : ExceptionSpecification
    {
        Establish context = () =>
        {
            machine.In(Values.Source)
                .ExecuteOnExit(() => { throw Values.Exception; })
                .On(Values.Event).Goto(Values.Destination);
        };

        Because of = () =>
        {
            machine.Initialize(Values.Source);
            machine.Start();
            machine.Fire(Values.Event, Values.Parameter);
        };

        Behaves_like<TransitionExceptionBehaviors> transition_exception_catcher;
    }

    [Subject(Concern.ExceptionHandling)]
    public class When_a_guard_in_a_transition_throws_an_exception : ExceptionSpecification
    {
        Establish context = () =>
        {
            machine.In(Values.Source)
                .On(Values.Event).If(() => { throw Values.Exception; }).Goto(Values.Destination);
        };

        Because of = () =>
        {
            machine.Initialize(Values.Source);
            machine.Start();
            machine.Fire(Values.Event, Values.Parameter);
        };

        Behaves_like<TransitionExceptionBehaviors> transition_exception_catcher;
    }

    [Subject(Concern.ExceptionHandling)]
    public class When_initializing_the_state_machine_and_an_entry_action_throws_an_exception : ExceptionSpecification
    {
        private const int State = 1;

        static readonly Exception Exception = new Exception();

        Establish context = () =>
            {
                machine.In(State)
                    .ExecuteOnEntry(() => { throw Exception; });
            };

        Because of = () =>
            {
                machine.Initialize(State);
                machine.Start();
            };

        It should_fire_exception_throw_event = () =>
            {
                receivedTransitionExceptionEventArgs.Exception.Should().NotBeNull();
            };

        It should_pass_thrown_exception_to_event_arguments_of_exception_thrown_event = () =>
            {
                receivedTransitionExceptionEventArgs.Exception.Should().BeSameAs(Exception);
            };
    }

    [Subject(Concern.ExceptionHandling)]
    public class When_transition_throws_exception_and_no_transition_exception_even_handler_is_registered
    {
        static readonly Exception Exception = new Exception();

        static PassiveStateMachine<int, int> machine;

        static Exception catchedException;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(Values.Source)
                    .On(Values.Event).Execute(() => { throw Exception; });

                machine.Initialize(Values.Source);
                machine.Start();
            };

        Because of = () =>
            {
                catchedException = Catch.Exception(() => machine.Fire(Values.Event));
            };

        It should_throw_exception = () =>
            catchedException.InnerException
                .Should().BeSameAs(Exception);
    }

    [Subject(Concern.ExceptionHandling)]
    public class When_exception_is_thrown_and_no_exception_even_handler_is_registered
    {
        static readonly Exception Exception = new Exception();

        static PassiveStateMachine<int, int> machine;

        static Exception catchedException;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();

            machine.In(Values.Source)
                .ExecuteOnEntry(() => { throw Exception; });

            machine.Initialize(Values.Source);
        };

        Because of = () =>
        {
            catchedException = Catch.Exception(() => machine.Start());
        };

        It should_throw_exception = () => 
            catchedException.InnerException
                .Should().BeSameAs(Exception);
    }

    [Behaviors]
    public class TransitionExceptionBehaviors
    {
        protected static TransitionExceptionEventArgs<int, int> receivedTransitionExceptionEventArgs;

        It should_catch_exception_and_fire_transition_exception_event = () =>
            {
                receivedTransitionExceptionEventArgs.Should().NotBeNull();
            };

        It should_pass_source_state_of_failing_transition_to_event_arguments_of_transition_exception_event = () =>
            {
                receivedTransitionExceptionEventArgs.StateId.Should().Be(Values.Source);
            };

        It should_pass_event_id_causing_transition_to_event_arguments_of_transition_exception_event = () =>
            {
                receivedTransitionExceptionEventArgs.EventId.Should().Be(Values.Event);
            };

        It should_pass_thrown_exception_to_event_arguments_of_transition_exception_event = () =>
            {
                receivedTransitionExceptionEventArgs.Exception.Should().BeSameAs(Values.Exception);
            };

        It should_pass_event_parameter_to_event_argument_of_transition_exception_event = () =>
            {
                receivedTransitionExceptionEventArgs.EventArgument.Should().Be(Values.Parameter);
            };
    }

    public class ExceptionSpecification
    {
        protected static PassiveStateMachine<int, int> machine;
        protected static TransitionExceptionEventArgs<int, int> receivedTransitionExceptionEventArgs;

        // ReSharper disable once UnusedMember.Local because it is a base spec
        Establish context = () =>
            {
                receivedTransitionExceptionEventArgs = null;

                machine = new PassiveStateMachine<int, int>();

                machine.TransitionExceptionThrown += (s, e) => receivedTransitionExceptionEventArgs = e;
            };
    }

    public static class Values
    {
        public const int Source = 1;
        public const int Destination = 2;
        public const int Event = 0;

        public const string Parameter = "oh oh";

        public static readonly Exception Exception = new Exception();
    }
}