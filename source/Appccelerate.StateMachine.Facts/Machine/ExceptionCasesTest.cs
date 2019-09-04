//-------------------------------------------------------------------------------
// <copyright file="ExceptionCasesTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine
{
    using System;
    using Appccelerate.StateMachine.Machine;
    using FluentAssertions;
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
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>().Build();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            Action action = () => testee.Fire(Events.A, stateContainer, stateContainer, stateDefinitions);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ExceptionIfNotInitialized_WhenAccessingCurrentState()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();

            Action action = () => { var state = stateContainer.CurrentStateId; };

            action.Should().Throw<NullReferenceException>();
        }

        /// <summary>
        /// When the state machine is initialized twice then an exception is thrown.
        /// </summary>
        [Fact]
        public void ExceptionIfInitializeIsCalledTwice()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);

            Action action = () => testee.Initialize(StateMachine.States.B, stateContainer, stateContainer);

            action.Should().Throw<InvalidOperationException>();
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
            StateMachine.States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B)
                        .If(() => throw exception)
                        .Goto(StateMachine.States.B))
                .Build();

            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
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

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.B, eventArguments, stateContainer, stateContainer, stateDefinitions);

            recordedStateId.Should().Be(StateMachine.States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().Be(StateMachine.States.A);
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
            StateMachine.States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B)
                        .Goto(StateMachine.States.B)
                        .Execute(() => throw exception))
                .Build();

            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.B, eventArguments, stateContainer, stateContainer, stateDefinitions);

            recordedStateId.Should().Be(StateMachine.States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public void EntryActionWhenThrowingExceptionThenNotificationAndStateIsEntered()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();
            StateMachine.States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B)
                        .Goto(StateMachine.States.B))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.B)
                        .ExecuteOnEntry(() => throw exception))
                .Build();

            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.B, eventArguments, stateContainer, stateContainer, stateDefinitions);

            recordedStateId.Should().Be(StateMachine.States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public void ExitActionWhenThrowingExceptionThenNotificationAndStateIsEntered()
        {
            var eventArguments = new object[] { 1, 2, "test" };
            var exception = new Exception();
            StateMachine.States? recordedStateId = null;
            Events? recordedEventId = null;
            object recordedEventArgument = null;
            Exception recordedException = null;

            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .ExecuteOnExit(() => throw exception)
                        .On(Events.B)
                        .Goto(StateMachine.States.B))
                .Build();

            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (sender, eventArgs) =>
            {
                recordedStateId = eventArgs.StateId;
                recordedEventId = eventArgs.EventId;
                recordedEventArgument = eventArgs.EventArgument;
                recordedException = eventArgs.Exception;
            };

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.B, eventArguments, stateContainer, stateContainer, stateDefinitions);
            recordedStateId.Should().Be(StateMachine.States.A);
            recordedEventId.Should().Be(Events.B);
            recordedEventArgument.Should().Be(eventArguments);
            recordedException.Should().Be(exception);
            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        /// <summary>
        /// The state machine has to be initialized before events can be fired.
        /// </summary>
        [Fact]
        public void NotInitialized()
        {
            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .Build();

            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
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
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.DefineHierarchyOn(StateMachine.States.A)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(StateMachine.States.B));

            Func<StateMachineDefinition<StateMachine.States, Events>> func =
                () => stateMachineDefinitionBuilder
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(StateMachine.States.C)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(StateMachine.States.B))
                    .Build();

            func.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MultipleTransitionsWithoutGuardsWhenDefiningAGotoThenInvalidOperationException()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B).If(() => false).Goto(StateMachine.States.C)
                        .On(Events.B).Goto(StateMachine.States.B));

            Func<StateMachineDefinition<StateMachine.States, Events>> func =
                () => stateMachineDefinitionBuilder
                    .WithConfiguration(x =>
                        x.In(StateMachine.States.A)
                            .On(Events.B)
                            .Goto(StateMachine.States.C))
                    .Build();

            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void MultipleTransitionsWithoutGuardsWhenDefiningAnActionThenInvalidOperationException()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B).Goto(StateMachine.States.B));

            Func<StateMachineDefinition<StateMachine.States, Events>> func =
                () => stateMachineDefinitionBuilder
                    .WithConfiguration(x =>
                        x.In(StateMachine.States.A)
                            .On(Events.B)
                            .Execute(() => { }))
                    .Build();

            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.OnlyOneTransitionMayHaveNoGuard);
        }

        [Fact]
        public void TransitionWithoutGuardHasToBeLast()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B).Goto(StateMachine.States.B));

            Func<StateMachineDefinition<StateMachine.States, Events>> func =
                () => stateMachineDefinitionBuilder
                    .WithConfiguration(x =>
                        x.In(StateMachine.States.A)
                            .On(Events.B)
                            .If(() => false)
                            .Execute(() => { }))
                    .Build();

            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.TransitionWithoutGuardHasToBeLast);
        }
    }
}