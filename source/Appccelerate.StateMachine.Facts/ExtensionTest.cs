//-------------------------------------------------------------------------------
// <copyright file="ExtensionTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts
{
    using System;
    using Extensions;
    using FakeItEasy;
    using Machine;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using StateMachine.Machine.Transitions;
    using Xunit;

    /// <summary>
    /// Tests that the extensions can interact with the state machine.
    /// </summary>
    public class ExtensionTest
    {
        [Fact]
        public void EnterInitialState()
        {
            const States InitialState = States.A;

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder.In(InitialState);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, InitialState);

            A.CallTo(() => extension.EnteringInitialState(InitialState))
                .MustHaveHappened();

            A.CallTo(() => extension.EnteredInitialState(
                    InitialState,
                    A<ITransitionContext<States, Events>>.That.Matches(context => context.StateDefinition == null)))
                .MustHaveHappened();
        }

        /// <summary>
        /// When an event is fired on the state machine then the extensions are notified.
        /// </summary>
        [Fact]
        public void Fire()
        {
            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B)
                .Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            var eventId = Events.B;
            var eventArgument = new object();

            testee.Fire(eventId, eventArgument, stateContainer, stateDefinitions);

            A.CallTo(() => extension.FiringEvent(ref eventId, ref eventArgument))
                .MustHaveHappened();

            A.CallTo(() => extension.FiredEvent(
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
            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.C)
                    .On(Events.C).Goto(States.C);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            const Events NewEvent = Events.C;
            var newEventArgument = new object();

            overrideExtension.OverriddenEvent = NewEvent;
            overrideExtension.OverriddenEventArgument = newEventArgument;

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.FiredEvent(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(Source)
                    .On(Event).Goto(Target);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Event, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.ExecutingTransition(
                    A<ITransitionDefinition<States, Events>>.That.Matches(
                        t => t.Source.Id == Source && t.Target.Id == Target),
                    A<ITransitionContext<States, Events>>.That.Matches(
                        c => c.EventId.Value == Event && c.StateDefinition.Id == Source)))
                .MustHaveHappened();

            A.CallTo(() => extension.ExecutedTransition(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .If(() => throw exception)
                    .Execute(() => { });
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandlingGuardException(
                     Transition(States.A),
                     Context(States.A, Events.B),
                     ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledGuardException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .If(() => throw exception)
                    .Execute(() => { });
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandledGuardException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Execute(() => throw exception);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandlingTransitionException(
                    Transition(States.A),
                    Context(States.A, Events.B),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledTransitionException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Execute(() => throw exception);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandledTransitionException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B);
            stateDefinitionBuilder
                .In(States.B)
                    .ExecuteOnEntry(() => throw exception);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandlingEntryActionException(
                    State(States.B),
                    Context(States.A, Events.B),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledEntryActionException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B);
            stateDefinitionBuilder
                .In(States.B)
                    .ExecuteOnEntry(() => throw exception);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandledEntryActionException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnExit(() => throw exception)
                    .On(Events.B)
                    .Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandlingExitActionException(
                    State(States.A),
                    Context(States.A, Events.B),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledExitActionException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var overrideExtension = new OverrideExtension();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);
            stateContainer.Extensions.Add(overrideExtension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnExit(() => throw exception)
                    .On(Events.B)
                    .Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            overrideExtension.OverriddenException = overriddenException;

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            A.CallTo(() => extension.HandledExitActionException(
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

            var extension = A.Fake<IExtensionInternal<States, Events>>();
            var stateContainer = new StateContainer<States, Events>();
            stateContainer.Extensions.Add(extension);

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnEntry(() => throw exception);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.TransitionExceptionThrown += (s, e) => { };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            A.CallTo(() => extension.HandlingEntryActionException(
                    State(States.A),
                    A<ITransitionContext<States, Events>>.That.Matches(context => context.StateDefinition == null),
                    ref exception))
                .MustHaveHappened();

            A.CallTo(() => extension.HandledEntryActionException(
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

        private class OverrideExtension : InternalExtensionBase<States, Events>
        {
            public States? OverriddenState { get; set; }

            public Events? OverriddenEvent { get; set; }

            public object OverriddenEventArgument { get; set; }

            public Exception OverriddenException { get; set; }

            public override void FiringEvent(ref Events eventId, ref object eventArgument)
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

            public override void HandlingGuardException(ITransitionDefinition<States, Events> transition, ITransitionContext<States, Events> transitionContext, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }

            public override void HandlingTransitionException(ITransitionDefinition<States, Events> transition, ITransitionContext<States, Events> context, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }

            public override void HandlingEntryActionException(IStateDefinition<States, Events> stateDefinition, ITransitionContext<States, Events> context, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }

            public override void HandlingExitActionException(IStateDefinition<States, Events> stateDefinition, ITransitionContext<States, Events> context, ref Exception exception)
            {
                if (this.OverriddenException != null)
                {
                    exception = this.OverriddenException;
                }
            }
        }
    }
}