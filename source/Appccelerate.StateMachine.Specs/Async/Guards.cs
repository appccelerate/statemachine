//-------------------------------------------------------------------------------
// <copyright file="Guards.cs" company="Appccelerate">
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
    using FluentAssertions;
    using Xbehave;

    public class Guards
    {
        private const int SourceState = 1;
        private const int DestinationState = 2;
        private const int ErrorState = 3;

        private const int Event = 2;

        [Scenario]
        public void MatchingGuard(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension currentStateExtension)
        {
            "establish a state machine with guarded transitions".x(async () =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                        .On(Event)
                            .If(() => false).Goto(ErrorState)
                            .If(async () => await Task.FromResult(false)).Goto(ErrorState)
                            .If(async () => await Task.FromResult(true)).Goto(DestinationState)
                            .If(() => true).Goto(ErrorState)
                            .Otherwise().Goto(ErrorState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                await machine.Start();
            });

            "when an event is fired".x(()
                => machine.Fire(Event));

            "it should take transition guarded with first matching guard".x(()
                => currentStateExtension.CurrentState.Should().Be(DestinationState));
        }

        [Scenario]
        public void NoMatchingGuard(
            AsyncPassiveStateMachine<int, int> machine)
        {
            var declined = false;

            "establish state machine with no matching guard".x(async () =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                        .On(Event)
                        .If(() => Task.FromResult(false)).Goto(ErrorState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                var currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);
                machine.TransitionDeclined += (sender, e) => declined = true;

                await machine.Start();
            });

            "when an event is fired".x(()
                => machine.Fire(Event));

            "it should notify about declined transition".x(()
                => declined.Should().BeTrue("TransitionDeclined event should be fired"));
        }

        [Scenario]
        public void OtherwiseGuard(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension currentStateExtension)
        {
            "establish a state machine with otherwise guard and no matching other guard".x(async () =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                        .On(Event)
                            .If(() => Task.FromResult(false)).Goto(ErrorState)
                            .Otherwise().Goto(DestinationState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                await machine.Start();
            });

            "when an event is fired".x(()
                => machine.Fire(Event));

            "it should_take_transition_guarded_with_otherwise".x(()
                => currentStateExtension.CurrentState.Should().Be(DestinationState));
        }

        [Scenario]
        public void PassingArguments(
            AsyncPassiveStateMachine<int, int> machine,
            string receivedArgument,
            string receivedAsyncArgument)
        {
            const string Argument = "argument";

            "establish a state machine with guarded transitions using an argument".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                    .On(Event)
                    .If((string argument) =>
                    {
                        receivedArgument = argument;
                        return false;
                    }).Goto(DestinationState)
                    .If((string argument) =>
                    {
                        receivedAsyncArgument = argument;
                        return Task.FromResult(false);
                    }).Goto(DestinationState)
                    .Otherwise().Goto(ErrorState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.Start();
            });

            "when an event is fired".x(() =>
                machine.Fire(Event, Argument));

            "it should pass the argument to the sync guards".x(() =>
                receivedArgument.Should().Be(Argument));

            "it should pass the argument to the async guards".x(() =>
                receivedAsyncArgument.Should().Be(Argument));
        }

        [Scenario]
        public void ThrowingGuard(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension currentStateExtension,
            ExceptionExtension<int, int> exceptionExtension,
            Exception receivedException)
        {
            const string Argument = "argument";
            var exception = new Exception("oops");

            "establish a state machine with a transition guard that throws an exception".x(() =>
            {
                var stateMachineDefinitionBuilder = StateMachineBuilder.ForAsyncMachine<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                    .On(Event)
                    .If(() => ThrowException(exception)).Goto(5)
                    .Otherwise().Goto(DestinationState);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                exceptionExtension = new ExceptionExtension<int, int>();
                machine.AddExtension(exceptionExtension);

                machine.TransitionExceptionThrown += ( sender,  args) => receivedException = args.Exception;

                machine.Start();
            });

            "when the transition with the failing guard is executed".x(() =>
                machine.Fire(Event, Argument));

            "it should treat the guard as a non-matching guard and proceed with other guards".x(() =>
                currentStateExtension.CurrentState.Should().Be(DestinationState));

            "it should notify extensions about the guard exception and the extension should be able to change the exception".x(() =>
                exceptionExtension.GuardExceptions.Should().BeEquivalentTo(new WrappedException(exception)));

            "it should fire TransitionExceptionThrown event".x(() =>
                receivedException.Should().BeEquivalentTo(new WrappedException(exception)));
        }

        private static Task<bool> ThrowException(
            Exception exception)
        {
            throw exception;
        }
    }
}