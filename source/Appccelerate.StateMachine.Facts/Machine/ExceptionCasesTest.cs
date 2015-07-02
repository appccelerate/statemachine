//-------------------------------------------------------------------------------
// <copyright file="ExceptionCasesTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;

    using Appccelerate.StateMachine.Persistence;
    using Appccelerate.StateMachine.Syntax;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests exception behavior of the <see cref="StateMachine{TState,TEvent}"/>.
    /// </summary>
    public class ExceptionCasesTest
    {
        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        private StateMachine.States? recordedStateId;

        private StateMachine.Events? recordedEventId;

        private object recordedEventArgument;

        private Exception recordedException;

        public ExceptionCasesTest()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();

            this.testee.TransitionExceptionThrown += (sender, eventArgs) =>
                                                         {
                                                             this.recordedStateId = eventArgs.StateId;
                                                             this.recordedEventId = eventArgs.EventId;
                                                             this.recordedEventArgument = eventArgs.EventArgument;
                                                             this.recordedException = eventArgs.Exception;
                                                         };
        }

        /// <summary>
        /// When the state machine is not initialized then an exception is throw when firing events on it.
        /// </summary>
        [Fact]
        public void ExceptionIfNotInitialized()
        {
            Action action = () => this.testee.Fire(StateMachine.Events.A);
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ExceptionIfNotInitialized_WhenAccessingCurrentState()
        {
            Action action = () => { var state = this.testee.CurrentStateId; };

            action.ShouldThrow<InvalidOperationException>();
        }

        /// <summary>
        /// When the state machine is initialized twice then an exception is thrown
        /// </summary>
        [Fact]
        public void ExceptionIfInitializeIsCalledTwice()
        {
            this.testee.Initialize(StateMachine.States.A);

            Action action = () => this.testee.Initialize(StateMachine.States.B);

            action.ShouldThrow<InvalidOperationException>();
        }

        /// <summary>
        /// When a guard throws an exception then it is captured and the <see cref="StateMachine{TState,TEvent}.TransitionExceptionThrown"/> event is fired.
        /// The transition is not executed and if there is no other transition then the state machine remains in the same state.
        /// </summary>
        [Fact]
        public void ExceptionThrowingGuard()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();
            
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                    .If(() => { throw exception; }).Goto(StateMachine.States.B);

            bool transitionDeclined = false;
            this.testee.TransitionDeclined += (sender, e) => transitionDeclined = true;

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.AssertException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception);
            this.testee.CurrentStateId.Should().Be(StateMachine.States.A);
            transitionDeclined.Should().BeTrue("transition was not declined.");
        }

        /// <summary>
        /// When a transition throws an exception then the exception is captured and the <see cref="StateMachine{TState,TEvent}.TransitionExceptionThrown"/> event is fired.
        /// The transition is executed and the state machine is in the target state.
        /// </summary>
        [Fact]
        public void ExceptionThrowingAction()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B).Execute(() =>
                                                         {
                                                             throw exception;
                                                         });

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.AssertException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception);
            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public void EntryActionWhenThrowingExceptionThenNotificationAndStateIsEntered()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.In(StateMachine.States.B)
                .ExecuteOnEntry(() =>
                                    {
                                        throw exception;
                                    });

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.AssertException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception);
            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public void ExitActionWhenThrowingExceptionThenNotificationAndStateIsEntered()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            Exception exception = new Exception();

            this.testee.In(StateMachine.States.A)
                .ExecuteOnExit(() =>
                                   {
                                       throw exception;
                                   })
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, eventArguments);

            this.AssertException(StateMachine.States.A, StateMachine.Events.B, eventArguments, exception);
            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        /// <summary>
        /// The state machine has to be initialized before events can be fired.
        /// </summary>
        [Fact]
        public void NotInitialized()
        {
            Action action = () => this.testee.Fire(StateMachine.Events.B);
            action.ShouldThrow<InvalidOperationException>();
        }

        /// <summary>
        /// When a state is added to two super states then an exception is thrown.
        /// </summary>
        [Fact]
        public void DefineNonTreeHierarchy()
        {
            this.testee.DefineHierarchyOn(StateMachine.States.A)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(StateMachine.States.B);

            Action action = () =>
            {
                var x = this.testee.DefineHierarchyOn(StateMachine.States.C)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(StateMachine.States.B);
            };

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void MultipleTransitionsWithoutGuardsWhenDefiningAGotoThenInvalidOperationException()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).If(() => false).Goto(StateMachine.States.C)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            Action action = () => this.testee.In(StateMachine.States.A).On(StateMachine.Events.B).Goto(StateMachine.States.C);

            action.ShouldThrow<InvalidOperationException>().WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void MultipleTransitionsWithoutGuardsWhenDefiningAnActionThenInvalidOperationException()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            Action action = () => this.testee.In(StateMachine.States.A).On(StateMachine.Events.B).Execute(() => { });

            action.ShouldThrow<InvalidOperationException>().WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void TransitionWithoutGuardHasToBeLast()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            Action action = () => this.testee.In(StateMachine.States.A).On(StateMachine.Events.B).If(() => false).Execute(() => { });

            action.ShouldThrow<InvalidOperationException>().WithMessage(ExceptionMessages.TransitionWithoutGuardHasToBeLast);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenAlreadyInitialized()
        {
            this.testee.Initialize(StateMachine.States.A);
            Action action = () => this.testee.Load(A.Fake<IStateMachineLoader<StateMachine.States>>());

            action.ShouldThrow<InvalidOperationException>().WithMessage(ExceptionMessages.StateMachineIsAlreadyInitialized);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenSettingALastActiveStateThatIsNotASubState()
        {
            this.testee.DefineHierarchyOn(StateMachine.States.B)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(StateMachine.States.B1)
                .WithSubState(StateMachine.States.B2);

            var loader = A.Fake<IStateMachineLoader<StateMachine.States>>();

            A.CallTo(() => loader.LoadHistoryStates())
                .Returns(new Dictionary<StateMachine.States, StateMachine.States>
                             {
                                 { StateMachine.States.B, StateMachine.States.A }
                             });

            Action action = () => this.testee.Load(loader);

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage(ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
        }

        /// <summary>
        /// Asserts that the correct exception was notified.
        /// </summary>
        /// <param name="expectedStateId">The expected state id.</param>
        /// <param name="expectedEventId">The expected event id.</param>
        /// <param name="expectedEventArguments">The expected event arguments.</param>
        /// <param name="expectedException">The expected exception.</param>
        private void AssertException(StateMachine.States expectedStateId, StateMachine.Events expectedEventId, object[] expectedEventArguments, Exception expectedException)
        {
            this.recordedStateId.Should().Be(expectedStateId);
            this.recordedEventId.Should().Be(expectedEventId);
            this.recordedEventArgument.Should().Be(expectedEventArguments);
            this.recordedException.Should().Be(expectedException);
        }
    }
}