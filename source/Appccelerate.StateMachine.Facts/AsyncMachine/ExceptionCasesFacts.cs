//-------------------------------------------------------------------------------
// <copyright file="ExceptionCasesFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.Infrastructure;
    using Xunit;

    public class ExceptionCasesFacts
    {
        [Fact]
        public void ThrowsExceptionIfNotInitialized_WhenFiringAnEvent()
        {
            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitions = new StateDefinitionsBuilder<States, Events>().Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            Func<Task> action = async () =>
                await testee.Fire(Events.A, Missing.Value, stateContainer, stateDefinitions)
                    .ConfigureAwait(false);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsExceptionIfNotInitialized_WhenAccessingCurrentState()
        {
            var stateContainer = new StateContainer<States, Events>();

            Action action = () => { var state = stateContainer.CurrentStateId.ExtractOrThrow(); };

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.ValueNotInitialized);
        }

        [Fact]
        public async Task StaysInCurrentState_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            bool ThrowingGuard() => throw exception;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B)
                    .If(ThrowingGuard).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) => { };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.A));
        }

        [Fact]
        public async Task DeclinesTransition_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            bool ThrowingGuard() => throw exception;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B)
                    .If(ThrowingGuard).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            var transitionDeclined = false;
            testee.TransitionDeclined += (sender, e) => transitionDeclined = true;
            testee.TransitionExceptionThrown += (sender, eventArgs) => { };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            transitionDeclined
                .Should()
                .BeTrue("transition was not declined.");
        }

        [Fact]
        public async Task CapturesException_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            bool ThrowingGuard() => throw exception;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B)
                .If(ThrowingGuard).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            CapturedException capturedException = null;
            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                capturedException = new CapturedException(
                    eventArgs.StateId,
                    eventArgs.EventId,
                    eventArgs.EventArgument,
                    eventArgs.Exception);
            };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            capturedException
                .Should()
                .Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task ContinuesEvaluatingGuards_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            bool ThrowingGuard() => throw exception;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B)
                    .If(ThrowingGuard).Goto(States.B)
                    .If(() => true).Goto(States.C);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) => { };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.C));
        }

        [Fact]
        public async Task CapturesException_WhenAnTransitionActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B).Goto(States.B).Execute(() => throw exception);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            CapturedException capturedException = null;
            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                capturedException = new CapturedException(
                    eventArgs.StateId,
                    eventArgs.EventId,
                    eventArgs.EventArgument,
                    eventArgs.Exception);
            };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            capturedException
                .Should()
                .Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task StaysInCurrentState_WhenTransitionActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B).Goto(States.B).Execute(() => throw exception);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) => { };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        [Fact]
        public async Task CapturesException_WhenEntryActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B).Goto(States.B);
            stateDefinitionsBuilder
                .In(States.B)
                .ExecuteOnEntry(() => throw exception);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            CapturedException capturedException = null;
            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                capturedException = new CapturedException(
                    eventArgs.StateId,
                    eventArgs.EventId,
                    eventArgs.EventArgument,
                    eventArgs.Exception);
            };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            capturedException
                .Should()
                .Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task EntersState_WhenEntryActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B).Goto(States.B);
            stateDefinitionsBuilder
                .In(States.B)
                .ExecuteOnEntry(() => throw exception);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) => { };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        [Fact]
        public async Task CapturesException_WhenExitActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .ExecuteOnExit(() => throw exception)
                .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            CapturedException capturedException = null;
            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                capturedException = new CapturedException(
                    eventArgs.StateId,
                    eventArgs.EventId,
                    eventArgs.EventArgument,
                    eventArgs.Exception);
            };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            capturedException
                .Should()
                .Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task ExitsState_WhenExitActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .ExecuteOnExit(() => throw exception)
                .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) => { };

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        [Fact]
        public void ThrowsException_WhenFiringAnEventOntoAnNotInitializedStateMachine()
        {
            var stateDefinitions = new StateDefinitionsBuilder<States, Events>().Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            Func<Task> action = async () =>
                await testee.Fire(Events.B, Missing.Value, stateContainer, stateDefinitions)
                    .ConfigureAwait(false);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenDefiningNonTreeHierarchy()
        {
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .DefineHierarchyOn(States.A)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(States.B);

            Action action = () =>
            {
                stateDefinitionBuilder
                    .DefineHierarchyOn(States.C)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(States.B);
            };

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenInitialStateIsSuperStateItselfInAnHierarchy()
        {
            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();

            Action action = () => stateDefinitionsBuilder
                .DefineHierarchyOn(States.B)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(States.B)
                    .WithSubState(States.B1)
                    .WithSubState(States.B2);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsException_WhenMultipleTransitionsWithoutGuardsAreDefined_GotoCase()
        {
            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();

            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B).If(() => false).Goto(States.C)
                    .On(Events.B).Goto(States.B);

            Action action = () =>
                stateDefinitionsBuilder
                    .In(States.A)
                        .On(Events.B).Goto(States.C);

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void ThrowsException_WhenMultipleTransitionsWithoutGuardsAreDefined_ExecuteCase()
        {
            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();

            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B);

            Action action = () =>
                stateDefinitionsBuilder
                    .In(States.A)
                        .On(Events.B).Execute(() => { });

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void ThrowsException_WhenTransitionWithoutGuardIsNotDefinedLast()
        {
            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();

            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B);

            Action action = () =>
                stateDefinitionsBuilder
                    .In(States.A)
                        .On(Events.B).If(() => false).Execute(() => { });

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.TransitionWithoutGuardHasToBeLast);
        }

        private class CapturedException
        {
            public CapturedException(
                States? recordedStateId,
                Events? recordedEventId,
                object recordedEventArgument,
                Exception recordedException)
            {
                this.RecordedStateId = recordedStateId;
                this.RecordedEventId = recordedEventId;
                this.RecordedEventArgument = recordedEventArgument;
                this.RecordedException = recordedException;
            }

            private States? RecordedStateId { get; }

            private Events? RecordedEventId { get; }

            private object RecordedEventArgument { get; }

            private Exception RecordedException { get; }

            private bool Equals(CapturedException other)
            {
                return this.RecordedStateId == other.RecordedStateId && this.RecordedEventId == other.RecordedEventId && Equals(this.RecordedEventArgument, other.RecordedEventArgument) && Equals(this.RecordedException, other.RecordedException);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return this.Equals((CapturedException)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = this.RecordedStateId.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.RecordedEventId.GetHashCode();
                    hashCode = (hashCode * 397) ^ (this.RecordedEventArgument != null ? this.RecordedEventArgument.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.RecordedException != null ? this.RecordedException.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}