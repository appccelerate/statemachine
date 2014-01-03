//-------------------------------------------------------------------------------
// <copyright file="EntryExitActionSpecification.cs" company="Appccelerate">
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
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using global::Machine.Specifications;

    [Subject(Concern.EntryAndExitActions)]
    public class When_defining_an_entry_action
    {
        private const int State = 1;

        private static PassiveStateMachine<int, int> machine;

        private static bool entryActionExecuted;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<int, int>();
            };

        Because of = () =>
            {
                machine.In(State)
                    .ExecuteOnEntry(() => entryActionExecuted = true);

                machine.Initialize(State);
                machine.Start();
            };

        It should_execute_entry_action_on_entry = () => 
            entryActionExecuted.Should().BeTrue();
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_defining_an_entry_action_with_parameter
    {
        private const int State = 1;
        private const string Parameter = "parameter";

        private static PassiveStateMachine<int, int> machine;

        private static string parameter;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnEntryParametrized(p => parameter = p, Parameter);

            machine.Initialize(State);
            machine.Start();
        };

        It should_execute_entry_action_on_entry = () => 
            parameter.Should().NotBeNull();

        It should_pass_parameter_to_entry_action = () =>
            parameter.Should().Be(Parameter);
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_defining_multiple_entry_actions
    {
        private const int State = 1;

        private static PassiveStateMachine<int, int> machine;

        private static bool entryAction1Executed;
        private static bool entryAction2Executed;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnEntry(() => entryAction1Executed = true)
                .ExecuteOnEntry(() => entryAction2Executed = true);
            
            machine.Initialize(State);
            machine.Start();
        };

        It should_execute_all_entry_actions_on_entry = () =>
        {
            entryAction1Executed
                .Should().BeTrue();
            
            entryAction2Executed
                .Should().BeTrue();
        };
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_an_entry_action_of_several_entry_actions_throws_an_exception
    {
        private const int State = 1;

        private static readonly Exception Exception2 = new Exception();
        private static readonly Exception Exception3 = new Exception();
        private static readonly List<Exception> ReceivedException = new List<Exception>();

        private static PassiveStateMachine<int, int> machine;

        private static bool entryAction1Executed;

        private static bool entryAction2Executed;

        private static bool entryAction3Executed;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnEntry(() => entryAction1Executed = true)
                .ExecuteOnEntry(() =>
                        {
                            entryAction2Executed = true;
                            throw Exception2;
                        })
                .ExecuteOnEntry(() =>
                        {
                            entryAction3Executed = true;
                            throw Exception3;
                        });

            machine.TransitionExceptionThrown += (s, e) => ReceivedException.Add(e.Exception);

            machine.Initialize(State);
            machine.Start();
        };

        It should_execute_all_entry_actions_on_entry = () =>
        {
            entryAction1Executed
                .Should().BeTrue("action 1 should be executed");

            entryAction2Executed
                .Should().BeTrue("action 2 should be executed");
            
            entryAction3Executed
                .Should().BeTrue("action 3 should be executed");
        };

        It should_handle_all_exceptions_of_all_throwing_entry_actions = () =>
        {
            ReceivedException
                .Should().BeEquivalentTo(new object[] { Exception2, Exception3 });
        };
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_defining_an_exit_action
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;

        private static PassiveStateMachine<int, int> machine;

        private static bool exitActionExecuted;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnExit(() => exitActionExecuted = true)
                .On(Event).Goto(AnotherState);

            machine.Initialize(State);
            machine.Start();
            machine.Fire(Event);
        };

        It should_execute_exit_action_on_exit = () => 
            exitActionExecuted.Should().BeTrue();
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_defining_an_exit_action_with_parameter
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;
        private const string Parameter = "parameter";

        private static PassiveStateMachine<int, int> machine;

        private static string parameter;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnExitParametrized(p => parameter = p, Parameter)
                .On(Event).Goto(AnotherState);

            machine.Initialize(State);
            machine.Start();
            machine.Fire(Event);
        };

        It should_execute_exit_action_on_exit = () => 
            parameter.Should().NotBeNull();

        It should_pass_parameter_to_exit_action = () => 
            parameter.Should().Be(Parameter);
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_defining_multiple_exit_actions
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;

        private static PassiveStateMachine<int, int> machine;

        private static bool exitAction1Executed;
        private static bool exitAction2Executed;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnExit(() => exitAction1Executed = true)
                .ExecuteOnExit(() => exitAction2Executed = true)
                .On(Event).Goto(AnotherState);

            machine.Initialize(State);
            machine.Start();
            machine.Fire(Event);
        };

        It should_execute_all_exit_actions_on_exit = () =>
        {
            exitAction1Executed
                .Should().BeTrue();

            exitAction2Executed
                .Should().BeTrue();
        };
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_an_exit_action_of_several_exit_actions_throws_an_exception
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;

        private static readonly Exception Exception2 = new Exception();
        private static readonly Exception Exception3 = new Exception();
        private static readonly List<Exception> ReceivedException = new List<Exception>();

        private static PassiveStateMachine<int, int> machine;

        private static bool exitAction1Executed;

        private static bool exitAction2Executed;

        private static bool exitAction3Executed;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();
        };

        Because of = () =>
        {
            machine.In(State)
                .ExecuteOnExit(() => exitAction1Executed = true)
                .ExecuteOnExit(() =>
                    {
                        exitAction2Executed = true;
                        throw Exception2;
                    })
                .ExecuteOnExit(() =>
                    {
                        exitAction3Executed = true;
                        throw Exception3;
                    })
                .On(Event).Goto(AnotherState);

            machine.TransitionExceptionThrown += (s, e) => ReceivedException.Add(e.Exception);

            machine.Initialize(State);
            machine.Start();
            machine.Fire(Event);
        };

        It should_execute_all_exit_actions_on_exit = () =>
        {
            exitAction1Executed
                .Should().BeTrue();

            exitAction2Executed
                .Should().BeTrue();

            exitAction3Executed
                .Should().BeTrue();
        };

        It should_handle_all_exceptions_of_all_throwing_exit_actions = () =>
            ReceivedException
                .Should().BeEquivalentTo(new object[]
                    {
                        Exception2, Exception3
                    });
    }

    [Subject(Concern.EntryAndExitActions)]
    public class When_calling_fire_with_an_event_argument
    {
        private const int State = 1;
        private const int AnotherState = 2;
        private const int Event = 2;
        private const int Argument = 17;

        private static PassiveStateMachine<int, int> machine;

        private static int argumentPassedToEntryAction;
        private static int argumentPassedToExitAction;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(State)
                    .ExecuteOnExit((int argument) => argumentPassedToExitAction = argument)
                    .On(Event).Goto(AnotherState);

                machine.In(AnotherState)
                    .ExecuteOnEntry((int argument) => argumentPassedToEntryAction = argument);

                machine.Initialize(State);
                machine.Start();
            };

        Because of = () =>
            machine.Fire(Event, Argument);

        It should_pass_event_argument_to_entry_actions_that_take_an_argument = () =>
            argumentPassedToEntryAction.Should().Be(Argument);

        It should_pass_event_argument_to_exit_actions_that_take_an_argument = () =>
            argumentPassedToExitAction.Should().Be(Argument);
    }
}