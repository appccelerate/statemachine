//-------------------------------------------------------------------------------
// <copyright file="EntryActions.cs" company="Appccelerate">
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
    using System.Threading.Tasks;
    using FluentAssertions;

    using Xbehave;

    public class EntryActions
    {
        private const int State = 1;

        [Scenario]
        public void EntryAction(
            AsyncPassiveStateMachine<int, int> machine,
            bool entryActionExecuted,
            bool asyncEntryActionExecuted)
        {
            "establish a state machine with entry action on a state"._(() =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();

                    machine.In(State)
                        .ExecuteOnEntry(() => entryActionExecuted = true)
                        .ExecuteOnEntry(async () =>
                        {
                            asyncEntryActionExecuted = true;
                            await Task.Yield();
                        });
                });

            "when entering the state"._(async () =>
                {
                    machine.Initialize(State);
                    await machine.Start();
                });

            "it should execute the synchronous entry action"._(()
                => entryActionExecuted.Should().BeTrue());

            "it should execute the asynchronous entry action"._(()
                => asyncEntryActionExecuted.Should().BeTrue());
        }

        [Scenario]
        public void EntryActionWithParameter(
            AsyncPassiveStateMachine<int, int> machine,
            string receivedParameter,
            string asyncReceivedParameter)
        {
            const string parameter = "parameter";

            "establish a state machine with entry action with parameter on a state"._(() =>
            {
                machine = new AsyncPassiveStateMachine<int, int>();

                machine.In(State)
                    .ExecuteOnEntryParametrized(p => receivedParameter = p, parameter)
                    .ExecuteOnEntryParametrized(
                        async p =>
                        {
                            asyncReceivedParameter = p;
                            await Task.Yield();
                        },
                        parameter);
            });

            "when entering the state"._(async () =>
            {
                machine.Initialize(State);
                await machine.Start();
            });

            "it should execute the entry synchronous action"._(()
                => receivedParameter.Should().NotBeNull());

            "it should pass parameter to the synchronous entry action"._(()
                => receivedParameter.Should().Be(parameter));

            "it should execute the asynchronous entry action"._(()
                => asyncReceivedParameter.Should().NotBeNull());

            "it should pass parameter to the asynchronous entry action"._(()
                => asyncReceivedParameter.Should().Be(parameter));
        }

        [Scenario]
        public void MultipleEntryActions(
            AsyncPassiveStateMachine<int, int> machine,
            bool entryAction1Executed,
            bool asyncEntryAction1Executed,
            bool entryAction2Executed,
            bool asyncEntryAction2Executed)
        {
            "establish a state machine with several entry actions on a state"._(() =>
            {
                machine = new AsyncPassiveStateMachine<int, int>();

                machine.In(State)
                    .ExecuteOnEntry(() => entryAction1Executed = true)
                    .ExecuteOnEntry(async () =>
                    {
                        asyncEntryAction1Executed = true;
                        await Task.Yield();
                    })
                    .ExecuteOnEntry(() => entryAction2Executed = true)
                    .ExecuteOnEntry(async () =>
                    {
                        asyncEntryAction2Executed = true;
                        await Task.Yield();
                    });
            });

            "when entering the state"._(async () =>
            {
                machine.Initialize(State);
                await machine.Start();
            });

            "it should execute all entry actions"._(()
                => new[]
                    {
                        entryAction1Executed,
                        asyncEntryAction1Executed,
                        entryAction2Executed,
                        asyncEntryAction2Executed
                    }.Should().Equal(true, true, true, true));
        }

        [Scenario]
        public void ExceptionHandling(
            AsyncPassiveStateMachine<int, int> machine,
            bool entryAction1Executed,
            bool entryAction2Executed,
            bool entryAction3Executed,
            bool entryAction4Executed)
        {
            var exception2 = new Exception();
            var exception3 = new Exception();
            var exception4 = new Exception();
            var receivedException = new List<Exception>();

            "establish a state machine with several entry actions on a state and some of them throw an exception"._(() =>
            {
                machine = new AsyncPassiveStateMachine<int, int>();

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
                        })
                .ExecuteOnEntry(async () =>
                    {
                        entryAction4Executed = true;
                        await Task.Yield();
                        throw exception4;
                    });

                machine.TransitionExceptionThrown += (s, e) => receivedException.Add(e.Exception);
            });

            "when entering the state"._(async () =>
            {
                machine.Initialize(State);
                await machine.Start();
            });

            "it should execute all entry actions on entry"._(()
                => new[]
                {
                    entryAction1Executed,
                    entryAction2Executed,
                    entryAction3Executed,
                    entryAction4Executed
                }.Should().Equal(true, true, true, true));

            "it should handle all exceptions of all throwing entry actions by firing the TransitionExceptionThrown event"._(()
                => receivedException
                    .Should().BeEquivalentTo(exception2, exception3, exception4));
        }

        [Scenario]
        public void EventArgument(
            PassiveStateMachine<int, int> machine,
            int passedArgument,
            int asyncPassedArgument)
        {
            const int Event = 3;
            const int anotherState = 3;
            const int argument = 17;

            "establish a state machine with an entry action taking an event argument"._(() =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.In(State)
                    .On(Event).Goto(anotherState);

                machine.In(anotherState)
                    .ExecuteOnEntry((int a) => passedArgument = a)
                    .ExecuteOnEntry(async (int a) =>
                    {
                        asyncPassedArgument = argument;
                        await Task.Yield();
                    });
            });

            "when entering the state"._(() =>
            {
                machine.Initialize(State);
                machine.Start();
                machine.Fire(Event, argument);
            });

            "it should pass event argument to synchronousentry action"._(()
                => passedArgument.Should().Be(argument));

            "it should pass event argument to asynchronous entry action"._(()
                => asyncPassedArgument.Should().Be(argument));
        }
    }
}