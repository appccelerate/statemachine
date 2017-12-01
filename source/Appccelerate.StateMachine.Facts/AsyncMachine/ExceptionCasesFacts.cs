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

namespace Appccelerate.StateMachine.AsyncMachine
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Appccelerate.StateMachine.Persistence;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ExceptionCasesFacts
    {
        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        private CapturedException capturedException;

        public ExceptionCasesFacts()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();

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
            Func<Task> action = async () => await this.testee.Fire(StateMachine.Events.A, Missing.Value);
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsExceptionIfNotInitialized_WhenAccessingCurrentState()
        {
            // ReSharper disable once UnusedVariable
            Action action = () => { var state = this.testee.CurrentStateId; };

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenInitializeIsCalledTwice()
        {
            this.testee.Initialize(StateMachine.States.A);

            Action action = () => this.testee.Initialize(StateMachine.States.B);

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public async Task StaysInCurrentState_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                .If((Func<bool>)ThrowingGuard).Goto(StateMachine.States.B);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.A);
        }

        [Fact]
        public async Task DeclinesTransition_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                .If((Func<bool>)ThrowingGuard).Goto(StateMachine.States.B);

            bool transitionDeclined = false;
            this.testee.TransitionDeclined += (sender, e) => transitionDeclined = true;

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            transitionDeclined.Should().BeTrue("transition was not declined.");
        }

        [Fact]
        public async Task CapturesException_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                .If((Func<bool>)ThrowingGuard).Goto(StateMachine.States.B);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task ContinuesEvaluatingGuards_WhenAGuardThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            bool ThrowingGuard() => throw exception;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                .If((Func<bool>)ThrowingGuard).Goto(StateMachine.States.B)
                .If(() => true).Goto(StateMachine.States.C);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.C);
        }

        [Fact]
        public async Task CapturesException_WhenAnTransitioNActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B).Execute(() => throw exception);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task StaysInCurrentState_WhenTransitionActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B).Execute(() => throw exception);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public async Task CapturesException_WhenEntryActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.In(StateMachine.States.B)
                .ExecuteOnEntry(() => throw exception);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task EntersState_WhenEntryActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.In(StateMachine.States.B)
                .ExecuteOnEntry(() => throw exception);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public async Task CapturesException_WhenExitActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .ExecuteOnExit(() => throw exception)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.capturedException.Should().Be(new CapturedException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception));
        }

        [Fact]
        public async Task ExitsState_WhenExitActionThrowsAnException()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .ExecuteOnExit(() => throw exception)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            await this.testee.Initialize(StateMachine.States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public void ThrowsException_WhenFiringAnEventOntoAnNotInitializedStateMachine()
        {
            Func<Task> action = async () => await this.testee.Fire(StateMachine.Events.B, Missing.Value);
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenDefiningNonTreeHierarchy()
        {
            this.testee.DefineHierarchyOn(StateMachine.States.A)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(StateMachine.States.B);

            Action action = () =>
            {
                this.testee.DefineHierarchyOn(StateMachine.States.C)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(StateMachine.States.B);
            };

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsException_WhenInitialStateIsSuperStateItselfInAnHierarchy()
        {
            Action action = () => this.testee.DefineHierarchyOn(StateMachine.States.B)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(StateMachine.States.B)
                .WithSubState(StateMachine.States.B1)
                .WithSubState(StateMachine.States.B2);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsException_WhenMultipleTransitionsWithoutGuardsAreDefined_GotoCase()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).If(() => false).Goto(StateMachine.States.C)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            Action action = () => this.testee.In(StateMachine.States.A).On(StateMachine.Events.B).Goto(StateMachine.States.C);

            action.ShouldThrow<InvalidOperationException>().WithMessage(Machine.ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void ThrowsException_WhenMultipleTransitionsWithoutGuardsAreDefined_ExecuteCase()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            Action action = () => this.testee.In(StateMachine.States.A).On(StateMachine.Events.B).Execute(() => { });

            action.ShouldThrow<InvalidOperationException>().WithMessage(Machine.ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void ThrowsException_WhenTransitionWithoutGuardIsNotdefinedLast()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            Action action = () => this.testee.In(StateMachine.States.A).On(StateMachine.Events.B).If(() => false).Execute(() => { });

            action.ShouldThrow<InvalidOperationException>().WithMessage(Machine.ExceptionMessages.TransitionWithoutGuardHasToBeLast);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenAlreadyInitialized()
        {
            this.testee.Initialize(StateMachine.States.A);
            Func<Task> action = async () => await this.testee.Load(A.Fake<IAsyncStateMachineLoader<StateMachine.States>>());

            action.ShouldThrow<InvalidOperationException>().WithMessage(Machine.ExceptionMessages.StateMachineIsAlreadyInitialized);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenSettingALastActiveStateThatIsNotASubState()
        {
            this.testee.DefineHierarchyOn(StateMachine.States.B)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(StateMachine.States.B1)
                .WithSubState(StateMachine.States.B2);

            var loader = A.Fake<IAsyncStateMachineLoader<StateMachine.States>>();

            A.CallTo(() => loader.LoadHistoryStates())
                .Returns(new Dictionary<StateMachine.States, StateMachine.States>
                             {
                                 { StateMachine.States.B, StateMachine.States.A }
                             });

            Func<Task> action = async () => await this.testee.Load(loader);

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage(Machine.ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
        }

        private class CapturedException
        {
            public CapturedException(
                StateMachine.States? recordedStateId,
                StateMachine.Events? recordedEventId,
                object recordedEventArgument,
                Exception recordedException)
            {
                this.RecordedStateId = recordedStateId;
                this.RecordedEventId = recordedEventId;
                this.RecordedEventArgument = recordedEventArgument;
                this.RecordedException = recordedException;
            }

            private StateMachine.States? RecordedStateId { get; }

            private StateMachine.Events? RecordedEventId { get; }

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