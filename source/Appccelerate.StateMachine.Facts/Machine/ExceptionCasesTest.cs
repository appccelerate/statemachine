//-------------------------------------------------------------------------------
// <copyright file="ExceptionCasesTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine
{
    using System;
    using Appccelerate.StateMachine.Machine;
    using FluentAssertions;
    using StateMachine.Infrastructure;
    using Xunit;

    /// <summary>
    /// Tests exception behavior of the <see cref="StateMachine{TState,TEvent}"/>.
    /// </summary>
    public class ExceptionCasesTest
    {
        /// <summary>
        /// When the state machine is not initialized then an exception is throw when firing events on it.
        /// </summary>
        [Fact]
        public void ExceptionIfNotInitialized()
        {
            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitions = new StateDefinitionsBuilder<States, Events>().Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            Action action = () => testee.Fire(Events.A, stateContainer, stateContainer, stateDefinitions);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ExceptionIfNotInitialized_WhenAccessingCurrentState()
        {
            var stateContainer = new StateContainer<States, Events>();

            Action action = () => { var state = stateContainer.CurrentStateId.ExtractOrThrow(); };

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.ValueNotInitialized);
        }

        /// <summary>
        /// When a guard throws an exception then it is captured and the <see cref="StateMachine{TState,TEvent}.TransitionExceptionThrown"/> event is fired.
        /// The transition is not executed and if there is no other transition then the state machine remains in the same state.
        /// </summary>
        [Fact]
        public void ExceptionThrowingGuard()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();
            States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder.In(States.A)
                .On(Events.B)
                .If(() => throw exception)
                .Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            var transitionDeclined = false;
            testee.TransitionDeclined += (sender, e) => transitionDeclined = true;
            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions);

            recordedStateId.Should().Be(States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().BeEquivalentTo(Initializable<States>.Initialized(States.A));
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
            var exception = new Exception();
            States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B)
                    .Execute(() => throw exception);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions);

            recordedStateId.Should().Be(States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        [Fact]
        public void EntryActionWhenThrowingExceptionThenNotificationAndStateIsEntered()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();
            States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B);
            stateDefinitionsBuilder
                .In(States.B)
                    .ExecuteOnEntry(() => throw exception);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions);

            recordedStateId.Should().Be(States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        [Fact]
        public void ExitActionWhenThrowingExceptionThenNotificationAndStateIsEntered()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();
            States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .ExecuteOnExit(() => throw exception)
                    .On(Events.B)
                    .Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, eventArguments, stateContainer, stateDefinitions);
            recordedStateId.Should().Be(States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        /// <summary>
        /// The state machine has to be initialized before events can be fired.
        /// </summary>
        [Fact]
        public void NotInitialized()
        {
            var stateDefinitions = new StateDefinitionsBuilder<States, Events>()
                .Build();

            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            Action action = () => testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);
            action.Should().Throw<InvalidOperationException>();
        }

        /// <summary>
        /// When a state is added to two super states then an exception is thrown.
        /// </summary>
        [Fact]
        public void DefineNonTreeHierarchy()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.A)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(States.B);

            Action action =
                () =>
                    stateMachineDefinitionBuilder
                        .DefineHierarchyOn(States.C)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(States.B);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MultipleTransitionsWithoutGuardsWhenDefiningAGotoThenInvalidOperationException()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.B).If(() => false).Goto(States.C)
                    .On(Events.B).Goto(States.B);

            Action action =
                () =>
                    stateMachineDefinitionBuilder
                        .In(States.A)
                            .On(Events.B)
                            .Goto(States.C);

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void MultipleTransitionsWithoutGuardsWhenDefiningAnActionThenInvalidOperationException()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B);

            Action action =
                () =>
                    stateMachineDefinitionBuilder
                        .In(States.A)
                            .On(Events.B)
                            .Execute(() => { });

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void TransitionWithoutGuardHasToBeLast()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B);

            Action action =
                () =>
                    stateMachineDefinitionBuilder
                        .In(States.A)
                            .On(Events.B)
                            .If(() => false)
                            .Execute(() => { });

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.TransitionWithoutGuardHasToBeLast);
        }
    }
}