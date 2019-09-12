//-------------------------------------------------------------------------------
// <copyright file="ExceptionHandling.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Async
{
    using System;
    using System.Threading.Tasks;
    using AsyncMachine;
    using AsyncMachine.Events;
    using FluentAssertions;
    using Specs;
    using Xbehave;

    public class ExceptionHandling
    {
        private TransitionExceptionEventArgs<int, int> receivedTransitionExceptionEventArgs;

        [Scenario]
        public void TransitionActionException(AsyncPassiveStateMachine<int, int> machine)
        {
            "establish a transition action throwing an exception".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(Values.Source)
                        .On(Values.Event)
                        .Goto(Values.Destination)
                        .Execute(() => throw Values.Exception);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(Values.Source)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.TransitionExceptionThrown += (s, e) => this.receivedTransitionExceptionEventArgs = e;
            });

            "when executing the transition".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Values.Event, Values.Parameter);
            });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void EntryActionException(AsyncPassiveStateMachine<int, int> machine)
        {
            "establish an entry action throwing an exception".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(Values.Source)
                        .On(Values.Event)
                        .Goto(Values.Destination);
                stateMachineDefinitionBuilder
                    .In(Values.Destination)
                    .ExecuteOnEntry(() => throw Values.Exception);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(Values.Source)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.TransitionExceptionThrown += (s, e) => this.receivedTransitionExceptionEventArgs = e;
            });

            "when executing the transition".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Values.Event, Values.Parameter);
            });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void ExitActionException(AsyncPassiveStateMachine<int, int> machine)
        {
            "establish an exit action throwing an exception".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(Values.Source)
                        .ExecuteOnExit(() => throw Values.Exception)
                        .On(Values.Event).Goto(Values.Destination);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(Values.Source)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.TransitionExceptionThrown += (s, e) => this.receivedTransitionExceptionEventArgs = e;
            });

            "when executing the transition".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Values.Event, Values.Parameter);
            });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void GuardException(AsyncPassiveStateMachine<int, int> machine)
        {
            "establish a guard throwing an exception".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(Values.Source)
                        .On(Values.Event)
                            .If((Func<Task<bool>>)(() => throw Values.Exception))
                            .Goto(Values.Destination);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(Values.Source)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.TransitionExceptionThrown += (s, e) => this.receivedTransitionExceptionEventArgs = e;
            });

            "when executing the transition".x(async () =>
            {
                await machine.Start();
                await machine.Fire(Values.Event, Values.Parameter);
            });

            this.ItShouldHandleTransitionException();
        }

        [Scenario]
        public void StartingException(AsyncPassiveStateMachine<int, int> machine)
        {
            const int state = 1;

            "establish a entry action for the initial state that throws an exception".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(state)
                    .ExecuteOnEntry(() => throw Values.Exception);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(Values.Source)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.TransitionExceptionThrown += (s, e) => this.receivedTransitionExceptionEventArgs = e;
            });

            "when starting the state machine".x(async () =>
            {
                await machine.Start();
            });

            "should catch exception and fire transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.Exception.Should().NotBeNull());

            "should pass thrown exception to event arguments of transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.Exception.Should().BeSameAs(Values.Exception));
        }

        [Scenario]
        public void NoExceptionHandlerRegistered(
            AsyncPassiveStateMachine<int, int> machine,
            Exception catchedException)
        {
            "establish an exception throwing state machine without a registered exception handler".x(async () =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(Values.Source)
                        .On(Values.Event).Execute(() => throw Values.Exception);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(Values.Source)
                    .Build()
                    .CreatePassiveStateMachine();

                await machine.Start();
            });

            "when an exception occurs".x(async () =>
                catchedException = await Catch.Exception(async () =>
                    await machine.Fire(Values.Event)));

            "should (re-)throw exception".x(() =>
                catchedException.InnerException
                    .Should().BeSameAs(Values.Exception));
        }

        private void ItShouldHandleTransitionException()
        {
            "should catch exception and fire transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.Should().NotBeNull());

            "should pass source state of failing transition to event arguments of transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.StateId.Should().Be(Values.Source));

            "should pass event id causing transition to event arguments of transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.EventId.Should().Be(Values.Event));

            "should pass thrown exception to event arguments of transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.Exception.Should().BeSameAs(Values.Exception));

            "should pass event parameter to event argument of transition exception event".x(() =>
                this.receivedTransitionExceptionEventArgs.EventArgument.Should().Be(Values.Parameter));
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
}