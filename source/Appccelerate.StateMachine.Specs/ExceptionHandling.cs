//-------------------------------------------------------------------------------
// <copyright file="ExceptionHandling.cs" company="Appccelerate">
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
    using Appccelerate.StateMachine.Machine.Events;
    using FluentAssertions;
    using Xbehave;

    public class ExceptionHandling
    {
        private PassiveStateMachine<int, int> machine;
        private TransitionExceptionEventArgs<int, int> receivedTransitionExceptionEventArgs;

        [Background]
        public void Background()
        {
            this.receivedTransitionExceptionEventArgs = null;

            this.machine = new PassiveStateMachine<int, int>();

            this.machine.TransitionExceptionThrown += (s, e) => this.receivedTransitionExceptionEventArgs = e;
        }

        [Scenario]
        public void TransitionActionException()
        {
            "establish a transition action throwing an exception"._(() => 
                this.machine.In(Values.Source)
                    .On(Values.Event).Goto(Values.Destination).Execute(() =>
                        {
                            throw Values.Exception;
                        }));

            "when executing the transition"._(() =>
                {
                    machine.Initialize(Values.Source);
                    machine.Start();
                    machine.Fire(Values.Event, Values.Parameter);
                });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void EntryActionException()
        {
            "establish an entry action throwing an exception"._(() =>
                {
                    this.machine.In(Values.Source)
                        .On(Values.Event).Goto(Values.Destination);

                    this.machine.In(Values.Destination)
                        .ExecuteOnEntry(() =>
                            {
                                throw Values.Exception;
                            });
                });

            "when executing the transition"._(() =>
                {
                    this.machine.Initialize(Values.Source);
                    this.machine.Start();
                    this.machine.Fire(Values.Event, Values.Parameter);
                });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void ExitActionException()
        {
            "establish an exit action throwing an exception"._(() =>
                    this.machine.In(Values.Source)
                        .ExecuteOnExit(() =>
                            {
                                throw Values.Exception;
                            })
                        .On(Values.Event).Goto(Values.Destination));

            "when executing the transition"._(() =>
                {
                    this.machine.Initialize(Values.Source);
                    this.machine.Start();
                    this.machine.Fire(Values.Event, Values.Parameter);
                });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void GuardException()
        {
            "establish a guard throwing an exception"._(() =>
                    this.machine.In(Values.Source)
                        .On(Values.Event)
                            .If(() =>
                                {
                                    throw Values.Exception;
                                })
                                .Goto(Values.Destination));

            "when executing the transition"._(() =>
                {
                    this.machine.Initialize(Values.Source);
                    this.machine.Start();
                    this.machine.Fire(Values.Event, Values.Parameter);
                });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void InitializationException()
        {
            const int State = 1;

            "establish a entry action for the initial state that throws an exception"._(() =>
                this.machine.In(State)
                    .ExecuteOnEntry(() =>
                        {
                            throw Values.Exception;
                        }));

            "when initializing the state machine"._(() =>
                {
                    machine.Initialize(State);
                    machine.Start();
                });

            "should catch exception and fire transition exception event"._(() => 
                this.receivedTransitionExceptionEventArgs.Exception.Should().NotBeNull());

            "should pass thrown exception to event arguments of transition exception event"._(() => 
                this.receivedTransitionExceptionEventArgs.Exception.Should().BeSameAs(Values.Exception));
        }

        [Scenario]
        public void NoExceptionHandlerRegistered(
            Exception catchedException)
        {
            "establish an exception throwing state machine without a registered exception handler"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();

                    machine.In(Values.Source)
                        .On(Values.Event).Execute(() =>
                            {
                                throw Values.Exception;
                            });

                    machine.Initialize(Values.Source);
                    machine.Start();
                });

            "when an exception occurs"._(() =>
                catchedException = Catch.Exception(() => this.machine.Fire(Values.Event)));

            "should (re-)throw exception"._(() =>
                catchedException.InnerException
                    .Should().BeSameAs(Values.Exception));
        }

        private void ItShouldHandleTransitionException()
        {
            "should catch exception and fire transition exception event"._(() =>
                this.receivedTransitionExceptionEventArgs.Should().NotBeNull());

            "should pass source state of failing transition to event arguments of transition exception event"._(() =>
                this.receivedTransitionExceptionEventArgs.StateId.Should().Be(Values.Source));

            "should pass event id causing transition to event arguments of transition exception event"._(() =>
                this.receivedTransitionExceptionEventArgs.EventId.Should().Be(Values.Event));

            "should pass thrown exception to event arguments of transition exception event"._(() =>
                this.receivedTransitionExceptionEventArgs.Exception.Should().BeSameAs(Values.Exception));

            "should pass event parameter to event argument of transition exception event"._(() =>
                this.receivedTransitionExceptionEventArgs.EventArgument.Should().Be(Values.Parameter));
        }
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