//-------------------------------------------------------------------------------
// <copyright file="EntryActions.cs" company="Appccelerate">
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
    using System.Collections.Generic;

    using FluentAssertions;

    using Xbehave;

    public class EntryActions
    {
        private const int State = 1;

        [Scenario]
        public void EntryAction(
            PassiveStateMachine<int, int> machine,
            bool entryActionExecuted)
        {
            "establish a state machine with entry action on a state"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();

                    machine.In(State)
                        .ExecuteOnEntry(() => entryActionExecuted = true);
                });

            "when entering the state"._(() =>
                {
                    machine.Initialize(State);
                    machine.Start();
                });

            "it should execute the entry action"._(() => 
                entryActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void EntryActionWithParameter(
            PassiveStateMachine<int, int> machine,
            string parameter)
        {
            const string Parameter = "parameter";

            "establish a state machine with entry action with parameter on a state"._(() =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(State)
                    .ExecuteOnEntryParametrized(p => parameter = p, Parameter);
            });

            "when entering the state"._(() =>
            {
                machine.Initialize(State);
                machine.Start();
            });

            "it should execute the entry action"._(() =>
                parameter.Should().NotBeNull());

            "it should pass parameter to the entry action"._(() =>
                parameter.Should().Be(Parameter));
        }

        [Scenario]
        public void MultipleEntryActions(
            PassiveStateMachine<int, int> machine,
            bool entryAction1Executed,
            bool entryAction2Executed)
        {
            "establish a state machine with several entry actions on a state"._(() =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(State)
                    .ExecuteOnEntry(() => entryAction1Executed = true)
                    .ExecuteOnEntry(() => entryAction2Executed = true);
            });

            "when entering the state"._(() =>
            {
                machine.Initialize(State);
                machine.Start();
            });

            "It should execute all entry actions"._(() =>
            {
                entryAction1Executed
                    .Should().BeTrue("first action should be executed");

                entryAction2Executed
                    .Should().BeTrue("second action should be executed");
            });
        }

        [Scenario]
        public void ExceptionHandling(
            PassiveStateMachine<int, int> machine,
            bool entryAction1Executed,
            bool entryAction2Executed,
            bool entryAction3Executed)
        {
            var exception2 = new Exception();
            var exception3 = new Exception();
            var receivedException = new List<Exception>();

            "establish a state machine with several entry actions on a state and some of them throw an exception"._(() =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(State)
                .ExecuteOnEntry(() => entryAction1Executed = true)
                .ExecuteOnEntry(() =>
                        {
                            entryAction2Executed = true;
                            throw exception2;
                        })
                .ExecuteOnEntry(() =>
                        {
                            entryAction3Executed = true;
                            throw exception3;
                        });

                machine.TransitionExceptionThrown += (s, e) => receivedException.Add(e.Exception);
            });

            "when entering the state"._(() =>
            {
                machine.Initialize(State);
                machine.Start();
            });

            "it should execute all entry actions on entry"._(() =>
            {
                entryAction1Executed
                    .Should().BeTrue("action 1 should be executed");

                entryAction2Executed
                    .Should().BeTrue("action 2 should be executed");

                entryAction3Executed
                    .Should().BeTrue("action 3 should be executed");
            });

            "it should handle all exceptions of all throwing entry actions by firing the TransitionExceptionThrown event"._(() => 
                receivedException
                    .Should().BeEquivalentTo(new object[]
                                                 {
                                                     exception2, exception3
                                                 }));
        }

        [Scenario]
        public void EventArgument(
            PassiveStateMachine<int, int> machine,
            int passedArgument)
        {
            const int Event = 3;
            const int AnotherState = 3;
            const int Argument = 17;

            "establish a state machine with an entry action taking an event argument"._(() =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(State)
                    .On(Event).Goto(AnotherState);

                machine.In(AnotherState)
                    .ExecuteOnEntry((int argument) => passedArgument = argument);
            });

            "when entering the state"._(() =>
            {
                machine.Initialize(State);
                machine.Start();
                machine.Fire(Event, Argument);
            });

            "it should pass event argument to entry action"._(() =>
                passedArgument.Should().Be(Argument));
        }
    }
}