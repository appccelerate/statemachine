//-------------------------------------------------------------------------------
// <copyright file="ExceptionCasesFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FluentAssertions;
    using Persistence;
    using StateMachine.AsyncMachine;
    using Xunit;

    public class ExceptionCasesFacts
    {
        private readonly StateMachine<States, Events> testee;

        private CapturedException capturedException;

        public ExceptionCasesFacts()
        {
            this.testee = new StateMachine<States, Events>();

            this.testee.TransitionExceptionThrown += (sender, eventArgs) =>
                                                         {
                                                             this.capturedException = new CapturedException(
                                                                 eventArgs.StateId,
                                                                 eventArgs.EventId,
                                                                 eventArgs.EventArgument,
                                                                 eventArgs.Exception);
                                                         };
        }

        [Fact]
        public void ThrowsExceptionIfNotInitialized_WhenFiringAnEvent()
        {
            Func<Task> action = async () => await this.testee.Fire(Events.A, Missing.Value);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsExceptionIfNotInitialized_WhenAccessingCurrentState()
        {
            // ReSharper disable once UnusedVariable
            Action action = () => { var state = this.testee.CurrentStateId; };

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task ThrowsException_WhenInitializeIsCalledTwice()
        {
            await this.testee.Initialize(States.A);

            Func<Task> action = async () => await this.testee.Initialize(States.B);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task StaysInCurrentState_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(States.A)
                .On(Events.B)
                .If(ThrowingGuard).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(States.A);
        }

        [Fact]
        public async Task DeclinesTransition_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(States.A)
                .On(Events.B)
                .If(ThrowingGuard).Goto(States.B);

            bool transitionDeclined = false;
            this.testee.TransitionDeclined += (sender, e) => transitionDeclined = true;

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            transitionDeclined.Should().BeTrue("transition was not declined.");
        }

        [Fact]
        public async Task CapturesException_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(States.A)
                .On(Events.B)
                .If(ThrowingGuard).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task ContinuesEvaluatingGuards_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(States.A)
                .On(Events.B)
                .If(ThrowingGuard).Goto(States.B)
                .If(() => true).Goto(States.C);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(States.C);
        }

        [Fact]
        public async Task CapturesException_WhenAnTransitionActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(States.A)
                .On(Events.B).Goto(States.B).Execute(() => throw exception);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task StaysInCurrentState_WhenTransitionActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(States.A)
                .On(Events.B).Goto(States.B).Execute(() => throw exception);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(States.B);
        }

        [Fact]
        public async Task CapturesException_WhenEntryActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(States.A)
                .On(Events.B).Goto(States.B);

            this.testee.In(States.B)
                .ExecuteOnEntry(() => throw exception);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task EntersState_WhenEntryActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(States.A)
                .On(Events.B).Goto(States.B);

            this.testee.In(States.B)
                .ExecuteOnEntry(() => throw exception);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(States.B);
        }

        [Fact]
        public async Task CapturesException_WhenExitActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(States.A)
                .ExecuteOnExit(() => throw exception)
                .On(Events.B).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(States.A, Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task ExitsState_WhenExitActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(States.A)
                .ExecuteOnExit(() => throw exception)
                .On(Events.B).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(States.B);
        }

        [Fact]
        public void ThrowsException_WhenFiringAnEventOntoAnNotInitializedStateMachine()
        {
            Func<Task> action = async () => await this.testee.Fire(Events.B, Missing.Value);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenDefiningNonTreeHierarchy()
        {
            this.testee.DefineHierarchyOn(States.A)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.B);

            Action action = () =>
            {
                this.testee.DefineHierarchyOn(States.C)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(States.B);
            };

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenInitialStateIsSuperStateItselfInAnHierarchy()
        {
            Action action = () => this.testee.DefineHierarchyOn(States.B)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.B)
                .WithSubState(States.B1)
                .WithSubState(States.B2);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsException_WhenMultipleTransitionsWithoutGuardsAreDefined_GotoCase()
        {
            this.testee.In(States.A)
                .On(Events.B).If(() => false).Goto(States.C)
                .On(Events.B).Goto(States.B);

            Action action = () => this.testee.In(States.A).On(Events.B).Goto(States.C);

            action.Should().Throw<InvalidOperationException>().WithMessage(StateMachine.Machine.ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void ThrowsException_WhenMultipleTransitionsWithoutGuardsAreDefined_ExecuteCase()
        {
            this.testee.In(States.A)
                .On(Events.B).Goto(States.B);

            Action action = () => this.testee.In(States.A).On(Events.B).Execute(() => { });

            action.Should().Throw<InvalidOperationException>().WithMessage(StateMachine.Machine.ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void ThrowsException_WhenTransitionWithoutGuardIsNotDefinedLast()
        {
            this.testee.In(States.A)
                .On(Events.B).Goto(States.B);

            Action action = () => this.testee.In(States.A).On(Events.B).If(() => false).Execute(() => { });

            action.Should().Throw<InvalidOperationException>().WithMessage(StateMachine.Machine.ExceptionMessages.TransitionWithoutGuardHasToBeLast);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenAlreadyInitialized()
        {
            this.testee.Initialize(States.A);
            Func<Task> action = async () => await this.testee.Load(A.Fake<IAsyncStateMachineLoader<States>>());

            action.Should().Throw<InvalidOperationException>().WithMessage(StateMachine.Machine.ExceptionMessages.StateMachineIsAlreadyInitialized);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenSettingALastActiveStateThatIsNotASubState()
        {
            this.testee.DefineHierarchyOn(States.B)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(States.B1)
                .WithSubState(States.B2);

            var loader = A.Fake<IAsyncStateMachineLoader<States>>();

            A.CallTo(() => loader.LoadHistoryStates())
                .Returns(new Dictionary<States, States>
                             {
                                 { States.B, States.A }
                             });

            Func<Task> action = async () => await this.testee.Load(loader);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage(StateMachine.Machine.ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
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