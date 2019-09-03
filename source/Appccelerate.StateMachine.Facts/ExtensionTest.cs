//-------------------------------------------------------------------------------
// <copyright file="ExtensionTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using StateMachine.Machine.Transitions;
    using Xunit;

    /// <summary>
    /// Tests that the extensions can interact with the state machine.
    /// </summary>
    public class ExtensionTest
    {
        /// <summary>
        /// When the state machine is initialized then the extensions get notified.
        /// </summary>
        [Fact]
        public void Initialize()
        {
            var initialState = States.A;

            var information = A.Fake<IStateMachineInformation<States, Events>>();
            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var standardFactory = new StandardFactory<States, Events>();
            var testee = new StateMachine<States, Events>(
                standardFactory,
                A.Fake<IStateLogic<States, Events>>(),
                new Dictionary<States, StateDefinition<States, Events>>());

            testee.Initialize(initialState, stateContainer, information);

            A.CallTo(() => extension.InitializingStateMachine(information, ref initialState))
                .MustHaveHappened();

            A.CallTo(() => extension.InitializedStateMachine(information, initialState))
                .MustHaveHappened();
        }

        [Fact]
        public void EnterInitialState()
        {
            const States InitialState = States.A;

            var information = A.Fake<IStateMachineInformation<States, Events>>();
            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(InitialState))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.Initialize(InitialState, stateContainer, information);
            testee.EnterInitialState(stateContainer, information);

            A.CallTo(() => extension.EnteringInitialState(information, InitialState))
                .MustHaveHappened();

            A.CallTo(() => extension.EnteredInitialState(
                    information,
                    InitialState,
                    A<ITransitionContext<States, Events>>.That.Matches(context => context.StateDefinition == null)))
                .MustHaveHappened();
        }

        /// <summary>
        /// An extension can override the state to which the state machine is initialized.
        /// </summary>
        [Fact]
        public void OverrideInitialState()
        {
            var overrideExtension = new OverrideExtension { OverriddenState = States.B };
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(overrideExtension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A))
                .WithConfiguration(x =>
                    x.In(States.B))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.Initialize(
                States.A,
                stateContainer,
                A.Fake<IStateMachineInformation<States, Events>>());

            testee.EnterInitialState(
                stateContainer,
                A.Fake<IStateMachineInformation<States, Events>>());

            stateContainer.CurrentStateId.Should().Be(States.B);
        }

        /// <summary>
        /// When an event is fired on the state machine then the extensions are notified.
        /// </summary>
        [Fact]
        public void Fire()
        {
            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .Goto(States.B))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            var eventId = Events.B;
            var eventArgument = new object();

            testee.Fire(eventId, eventArgument, stateContainer, stateContainer);

            A.CallTo(() => extension.FiringEvent(stateContainer, ref eventId, ref eventArgument))
                .MustHaveHappened();

            A.CallTo(() => extension.FiredEvent(
                    stateContainer,
                    A<ITransitionContext<States, Events>>.That.Matches(
                        context =>
                            context.StateDefinition.Id == States.A &&
                            context.EventId.Value == eventId &&
                            context.EventArgument == eventArgument)))
                .MustHaveHappened();
        }

        /// <summary>
        /// An extension can override the event id and the event arguments.
        /// </summary>
        [Fact]
        public void OverrideFiredEvent()
        {
            var extension = A.Fake<IExtension<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B).Goto(States.C)
                        .On(Events.C).Goto(States.C))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            const Events NewEvent = Events.C;
            var newEventArgument = new object();

            overrideExtension.OverriddenEvent = NewEvent;
            overrideExtension.OverriddenEventArgument = newEventArgument;

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.FiredEvent(
                stateContainer,
                A<ITransitionContext<States, Events>>.That.Matches(
                    c => c.EventId.Value == NewEvent && c.EventArgument == newEventArgument)))
                .MustHaveHappened();
        }

        [Fact]
        public void NotifiesAboutTransitions()
        {
            const States Source = States.A;
            const States Target = States.B;
            const Events Event = Events.B;

            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(Source)
                        .On(Event).Goto(Target))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.Initialize(Source, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            testee.Fire(Event, stateContainer, stateContainer);

            A.CallTo(() => extension.ExecutingTransition(
                    stateContainer,
                    A<ITransitionDefinition<States, Events>>.That.Matches(
                        t => t.Source.Id == Source && t.Target.Id == Target),
                    A<ITransitionContext<States, Events>>.That.Matches(
                        c => c.EventId.Value == Event && c.StateDefinition.Id == Source)))
                .MustHaveHappened();

            A.CallTo(() => extension.ExecutedTransition(
                    stateContainer,
                    A<ITransitionDefinition<States, Events>>.That.Matches(
                        t => t.Source.Id == Source && t.Target.Id == Target),
                    A<ITransitionContext<States, Events>>.That.Matches(
                        c => c.EventId.Value == Event && c.StateDefinition.Id == Source)))
                .MustHaveHappened();
        }

        /// <summary>
        /// Exceptions thrown by guards are passed to extensions.
        /// </summary>
        [Fact]
        public void ExceptionThrowingGuard()
        {
            var exception = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .If(() => throw exception)
                        .Execute(() => { }))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandlingGuardException(
                     stateContainer,
                     Transition(States.A),
                     Context(States.A, Events.B),
                     ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledGuardException(
                    stateContainer,
                    Transition(States.A),
                    Context(States.A, Events.B),
                    exception))
                .MustHaveHappened();
        }

        /// <summary>
        /// An extension can override the exception thrown by a transition guard.
        /// </summary>
        [Fact]
        public void OverrideGuardException()
        {
            var exception = new Exception();
            var overriddenException = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .If(() => throw exception)
                        .Execute(() => { }))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandledGuardException(
                stateContainer,
                A<ITransitionDefinition<States, Events>>._,
                A<ITransitionContext<States, Events>>._,
                overriddenException))
                .MustHaveHappened();
        }

        /// <summary>
        /// Exceptions thrown in actions are passed to the extensions.
        /// </summary>
        [Fact]
        public void ExceptionThrowingAction()
        {
            var exception = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .Execute(() => throw exception))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandlingTransitionException(
                    stateContainer,
                    Transition(States.A),
                    Context(States.A, Events.B),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledTransitionException(
                    stateContainer,
                    Transition(States.A),
                    Context(States.A, Events.B),
                    exception))
                .MustHaveHappened();
        }

        /// <summary>
        /// An extension can override the exception thrown by an action.
        /// </summary>
        [Fact]
        public void OverrideActionException()
        {
            var exception = new Exception();
            var overriddenException = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .Execute(() => throw exception))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandledTransitionException(
                    stateContainer,
                    A<ITransitionDefinition<States, Events>>._,
                    A<ITransitionContext<States, Events>>._,
                    overriddenException))
                .MustHaveHappened();
        }

        /// <summary>
        /// Exceptions thrown by entry actions are passed to extensions.
        /// </summary>
        [Fact]
        public void EntryActionException()
        {
            var exception = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .Goto(States.B))
                .WithConfiguration(x =>
                    x.In(States.B)
                        .ExecuteOnEntry(() => throw exception))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandlingEntryActionException(
                    stateContainer,
                    State(States.B),
                    Context(States.A, Events.B),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledEntryActionException(
                    stateContainer,
                    State(States.B),
                    Context(States.A, Events.B),
                    exception))
                .MustHaveHappened();
        }

        /// <summary>
        /// An extension can override the exception thrown by an entry action.
        /// </summary>
        [Fact]
        public void OverrideEntryActionException()
        {
            var exception = new Exception();
            var overriddenException = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .On(Events.B)
                        .Goto(States.B))
                .WithConfiguration(x =>
                    x.In(States.B)
                        .ExecuteOnEntry(() => throw exception))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandledEntryActionException(
                    stateContainer,
                    A<IStateDefinition<States, Events>>._,
                    A<ITransitionContext<States, Events>>._,
                    overriddenException))
                .MustHaveHappened();
        }

        /// <summary>
        /// Exceptions thrown by exit actions are passed to extensions.
        /// </summary>
        [Fact]
        public void ExitActionException()
        {
            var exception = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .ExecuteOnExit(() => throw exception)
                        .On(Events.B)
                        .Goto(States.B))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandlingExitActionException(
                    stateContainer,
                    State(States.A),
                    Context(States.A, Events.B),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledExitActionException(
                    stateContainer,
                    State(States.A),
                    Context(States.A, Events.B),
                    exception))
                .MustHaveHappened();
        }

        /// <summary>
        /// Exceptions thrown by exit actions can be overridden by extensions.
        /// </summary>
        [Fact]
        public void OverrideExitActionException()
        {
            var exception = new Exception();
            var overriddenException = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .ExecuteOnExit(() => throw exception)
                        .On(Events.B)
                        .Goto(States.B))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer);

            A.CallTo(() => extension.HandledExitActionException(
                    stateContainer,
                    A<IStateDefinition<States, Events>>._,
                    A<ITransitionContext<States, Events>>._,
                    overriddenException))
                .MustHaveHappened();
        }

        /// <summary>
        /// Exceptions thrown by entry actions during initialization are passed to extensions.
        /// </summary>
        [Fact]
        public void EntryActionExceptionDuringInitialization()
        {
            var exception = new Exception();

            var extension = A.Fake<IExtension<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var testee = new StateMachineDefinitionBuilder<States, Events>()
                .WithConfiguration(x =>
                    x.In(States.A)
                        .ExecuteOnEntry(() => throw exception))
                .Build()
                .CreateStateMachine(stateContainer);

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.Initialize(States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer);

            A.CallTo(() => extension.HandlingEntryActionException(
                    stateContainer,
                    State(States.A),
                    A<ITransitionContext<States, Events>>.That.Matches(context => context.StateDefinition == null),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledEntryActionException(
                    stateContainer,
                    State(States.A),
                    A<ITransitionContext<States, Events>>.That.Matches(context => context.StateDefinition == null),
                    exception))
                .MustHaveHappened();
        }

        private static IStateDefinition<States, Events> State(States stateId)
        {
            return A<IStateDefinition<States, Events>>.That.Matches(state => state.Id == stateId);
        }

        private static ITransitionDefinition<States, Events> Transition(States sourceState)
        {
            return A<ITransitionDefinition<States, Events>>.That.Matches(transition => transition.Source.Id == sourceState);
        }

        private static ITransitionContext<States, Events> Context(States sourceState, Events eventId)
        {
            return A<ITransitionContext<States, Events>>.That.Matches(context => context.EventId.Value == eventId && context.StateDefinition.Id == sourceState);
        }

        private class OverrideExtension : Extensions.ExtensionBase<States, Events>
        {
            public States? OverriddenState { get; set; }

            public Events? OverriddenEvent { get; set; }

            public object OverriddenEventArgument { get; set; }

            public Exception OverriddenException { get; set; }

            public override void InitializingStateMachine(IStateMachineInformation<States, Events> stateMachine, ref States initialState)
            {
                if (this.OverriddenState.HasValue)
                {
                    initialState = this.OverriddenState.Value;
                }
            }

            public override void FiringEvent(IStateMachineInformation<States, Events> stateMachine, ref Events eventId, ref object eventArgument)
            {
                if (this.OverriddenEvent.HasValue)
                {
                    eventId = this.OverriddenEvent.Value;
                }

                if (this.OverriddenEventArgument != null)
                {
                    eventArgument = this.OverriddenEventArgument;
                }
            }

            public override void HandlingGuardException(IStateMachineInformation<States, Events> stateMachine, ITransitionDefinition<States, Events> transition, ITransitionContext<States, Events> transitionContext, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }

            public override void HandlingTransitionException(IStateMachineInformation<States, Events> stateMachine, ITransitionDefinition<States, Events> transition, ITransitionContext<States, Events> context, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }

            public override void HandlingEntryActionException(IStateMachineInformation<States, Events> stateMachine, IStateDefinition<States, Events> stateDefinition, ITransitionContext<States, Events> context, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }

            public override void HandlingExitActionException(IStateMachineInformation<States, Events> stateMachine, IStateDefinition<States, Events> stateDefinition, ITransitionContext<States, Events> context, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }
        }
    }
}