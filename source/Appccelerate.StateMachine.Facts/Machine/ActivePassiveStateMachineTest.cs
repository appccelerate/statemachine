//-------------------------------------------------------------------------------
// <copyright file="ActivePassiveStateMachineTest.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using FakeItEasy;
    using FluentAssertions;
    using Persistence;
    using StateMachine.Infrastructure;
    using StateMachine.Machine;
    using StateMachine.Machine.Events;
    using Xunit;

    /// <summary>
    /// Tests the <see cref="PassiveStateMachine{TState,TEvent}"/> and <see cref="ActiveStateMachine{TState,TEvent}"/> class.
    /// </summary>
    public class ActivePassiveStateMachineTest
    {
        public static IEnumerable<object[]> StateMachineInstantiationProvider =>
            new List<object[]>
            {
                new object[] { "PassiveStateMachine", new Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>>(smd => smd.CreatePassiveStateMachine()) },
                new object[] { "ActiveStateMachine", new Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>>(smd => smd.CreateActiveStateMachine()) }
            };

        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void StartThenInitialStateIsEntered(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var enteredStateSignal = new AutoResetEvent(false);

            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .ExecuteOnEntry(() => enteredStateSignal.Set());
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);

            testee.Start();

            enteredStateSignal.WaitOne(1000)
                .Should().BeTrue();
        }

        /// <summary>
        /// The <see cref="IStateMachine{TState,TEvent}.IsRunning"/> reflects the state of the state machine.
        /// </summary>
        /// <param name="dummyName">Unused parameter. Just here to give each Theory test case a name.</param>
        /// <param name="createStateMachine">Function to create a IStateMachine from a StateMachineDefinition.</param>
        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void StartStop(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);

            var runningInitially = testee.IsRunning;
            testee.Start();
            var runningAfterStart = testee.IsRunning;
            testee.Stop();
            var runningAfterStop = testee.IsRunning;

            runningInitially.Should().BeFalse("initially");
            runningAfterStart.Should().BeTrue("after start");
            runningAfterStop.Should().BeFalse("after stop");
        }

        /// <summary>
        /// An event can be fired onto the state machine and all notifications are signaled.
        /// </summary>
        /// <param name="dummyName">Unused parameter. Just here to give each Theory test case a name.</param>
        /// <param name="createStateMachine">Function to create a IStateMachine from a StateMachineDefinition.</param>
        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void FireEvent(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var exceptions = new List<EventArgs>();
            var transitionBeginMessages = new List<TransitionEventArgs<States, Events>>();
            var transitionCompletedMessages = new List<TransitionCompletedEventArgs<States, Events>>();
            var transitionDeclinedMessages = new List<TransitionEventArgs<States, Events>>();

            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.B)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(States.B1)
                    .WithSubState(States.B2);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.C)
                    .WithHistoryType(HistoryType.Shallow)
                    .WithInitialSubState(States.C1)
                    .WithSubState(States.C2);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.C1)
                    .WithHistoryType(HistoryType.Shallow)
                    .WithInitialSubState(States.C1A)
                    .WithSubState(States.C1B);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.D)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(States.D1)
                    .WithSubState(States.D2);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.D1)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(States.D1A)
                    .WithSubState(States.D1B);
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);

            testee.TransitionExceptionThrown += (sender, e) => exceptions.Add(e);
            testee.TransitionBegin += (sender, e) => transitionBeginMessages.Add(e);
            testee.TransitionCompleted += (sender, e) => transitionCompletedMessages.Add(e);
            testee.TransitionDeclined += (sender, e) => transitionDeclinedMessages.Add(e);

            var allTransitionsCompleted = SetUpWaitForAllTransitions(testee, 1);

            object eventArgument = "test";

            testee.Start();

            testee.Fire(Events.B, eventArgument);

            WaitForAllTransitions(allTransitionsCompleted);

            transitionBeginMessages.Should().HaveCount(1);
            transitionBeginMessages.Single().StateId.Should().Be(States.A);
            transitionBeginMessages.Single().EventId.Should().Be(Events.B);
            transitionBeginMessages.Single().EventArgument.Should().Be(eventArgument);

            transitionCompletedMessages.Should().HaveCount(1);
            transitionCompletedMessages.Single().StateId.Should().Be(States.A);
            transitionCompletedMessages.Single().EventId.Should().Be(Events.B);
            transitionCompletedMessages[0].EventArgument.Should().Be(eventArgument);
            transitionCompletedMessages.Single().NewStateId.Should().Be(States.B1);

            exceptions.Should().BeEmpty();
            transitionDeclinedMessages.Should().BeEmpty();

            testee.Stop();
        }

        /// <summary>
        /// With FirePriority, an event can be added to the front of the queued events.
        /// </summary>
        /// <param name="dummyName">Unused parameter. Just here to give each Theory test case a name.</param>
        /// <param name="createStateMachine">Function to create a IStateMachine from a StateMachineDefinition.</param>
        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void PriorityFire(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            const int Transitions = 3;
            var exceptions = new List<EventArgs>();
            var transitionCompletedMessages = new List<TransitionCompletedEventArgs<States, Events>>();
            var transitionDeclinedMessages = new List<TransitionEventArgs<States, Events>>();

            IStateMachine<States, Events> testee = null;
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B).Execute(() =>
                    {
                        FireD();
                        FirePriorityC();
                    });
            stateMachineDefinitionBuilder
                .In(States.B)
                    .On(Events.C).Goto(States.C);
            stateMachineDefinitionBuilder
                .In(States.C)
                    .On(Events.D).Goto(States.D);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            testee = createStateMachine(stateMachineDefinition);

            void FireD() => testee.Fire(Events.D);
            void FirePriorityC() => testee.FirePriority(Events.C);

            testee.TransitionExceptionThrown += (sender, e) => exceptions.Add(e);
            testee.TransitionCompleted += (sender, e) => transitionCompletedMessages.Add(e);
            testee.TransitionDeclined += (sender, e) => transitionDeclinedMessages.Add(e);

            var allTransitionsCompleted = SetUpWaitForAllTransitions(testee, 1);

            testee.Start();

            testee.Fire(Events.B);

            WaitForAllTransitions(allTransitionsCompleted);

            transitionCompletedMessages.Count.Should().Be(Transitions);
            transitionDeclinedMessages.Should().BeEmpty();
            exceptions.Should().BeEmpty();

            testee.Stop();
        }

        /// <summary>
        /// When the state machine is stopped then no events are processed.
        /// All events queued are processed when state machine is started.
        /// </summary>
        /// <param name="dummyName">Unused parameter. Just here to give each Theory test case a name.</param>
        /// <param name="createStateMachine">Function to create a IStateMachine from a StateMachineDefinition.</param>
        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void StopAndRestart(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            const int Transitions = 2;
            var transitionBeginMessages = new List<TransitionEventArgs<States, Events>>();
            var transitionCompletedMessages = new List<TransitionCompletedEventArgs<States, Events>>();

            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.B).Goto(States.B);
            stateMachineDefinitionBuilder
                .In(States.B)
                    .On(Events.C).Goto(States.C);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);

            testee.TransitionBegin += (sender, e) => transitionBeginMessages.Add(e);
            testee.TransitionCompleted += (sender, e) => transitionCompletedMessages.Add(e);

            var allTransitionsCompleted = SetUpWaitForAllTransitions(testee, 1);

            testee.Start();

            testee.Stop();

            testee.IsRunning.Should().BeFalse("after stop, state machine should not be running.");

            testee.Fire(Events.B);
            testee.Fire(Events.C);

            transitionBeginMessages.Count.Should().Be(0);

            testee.Start();

            WaitForAllTransitions(allTransitionsCompleted);

            transitionCompletedMessages.Count.Should().Be(Transitions);

            testee.Stop();
        }

        /// <summary>
        /// The state machine can be started twice with no effect.
        /// </summary>
        /// <param name="dummyName">Unused parameter. Just here to give each Theory test case a name.</param>
        /// <param name="createStateMachine">Function to create a IStateMachine from a StateMachineDefinition.</param>
        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void StartTwice(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);

            testee.Start();
            testee.Start();

            testee.IsRunning.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void SetsCurrentStateOnLoadingFromPersistedState(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var loader = A.Fake<IStateMachineLoader<States, Events>>();
            var extension = A.Fake<IExtension<States, Events>>();

            A.CallTo(() => loader.LoadCurrentState())
                .Returns(Initializable<States>.Initialized(States.C));

            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A);
            stateMachineDefinitionBuilder
                .In(States.C);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);
            testee.AddExtension(extension);

            testee.Load(loader);

            A.CallTo(() =>
                    extension.Loaded(
                        A<IStateMachineInformation<States, Events>>.Ignored,
                        A<Initializable<States>>
                            .That
                            .Matches(currentState =>
                                currentState.IsInitialized
                                && currentState.ExtractOrThrow() == States.C),
                        A<IReadOnlyDictionary<States, States>>.Ignored,
                        A<IReadOnlyCollection<EventInformation<Events>>>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void SetsHistoryStatesOnLoadingFromPersistedState()
        {
            var exitedD2 = false;

            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.D)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(States.D1)
                    .WithSubState(States.D2);
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.D).Goto(States.D)
                    .On(Events.A);
            stateMachineDefinitionBuilder
                .In(States.D2)
                    .ExecuteOnExit(() => exitedD2 = true)
                    .On(Events.A).Goto(States.A);
            var testee = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build()
                .CreatePassiveStateMachine();

            var loader = A.Fake<IStateMachineLoader<States, Events>>();

            A.CallTo(() => loader.LoadHistoryStates())
                .Returns(new Dictionary<States, States>
                {
                    { States.D, States.D2 }
                });
            A.CallTo(() => loader.LoadCurrentState())
                .Returns(Initializable<States>.UnInitialized());

            testee.Load(loader);
            testee.Start();
            testee.Fire(Events.D); // should go to loaded last active state D2, not initial state D1
            exitedD2 = false;
            testee.Fire(Events.A);

            testee.Stop();

            exitedD2.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void SetsEventsOnLoadingFromPersistedState(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var enteredB = false;
            var extension = A.Fake<IExtension<States, Events>>();

            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A)
                    .On(Events.E)
                    .Goto(States.B);
            stateMachineDefinitionBuilder
                .In(States.B)
                    .ExecuteOnEntry(() => enteredB = true);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();
            var testee = createStateMachine(stateMachineDefinition);
            testee.AddExtension(extension);

            var loader = A.Fake<IStateMachineLoader<States, Events>>();

            var eventInformation = new EventInformation<Events>(Events.E, null);
            A.CallTo(() => loader.LoadEvents())
                .Returns(new List<EventInformation<Events>> { eventInformation });
            A.CallTo(() => loader.LoadCurrentState())
                .Returns(Initializable<States>.UnInitialized());

            testee.Load(loader);

            A.CallTo(() => extension.Loaded(
                    A<IStateMachineInformation<States, Events>>.Ignored,
                    A<Initializable<States>>.Ignored,
                    A<IReadOnlyDictionary<States, States>>.Ignored,
                    A<IReadOnlyCollection<EventInformation<Events>>>
                        .That
                        .Matches(c =>
                            c.Count == 1
                            && c.Contains(eventInformation))))
                .MustHaveHappenedOnceExactly();

            var signal = new AutoResetEvent(false);
            A.CallTo(() =>
                    extension.EnteredInitialState(
                        A<IStateMachineInformation<States, Events>>.Ignored,
                        A<States>.Ignored,
                        A<ITransitionContext<States, Events>>.Ignored))
                .Invokes(() => signal.Set());
            testee.AddExtension(extension);

            testee.Start();

            var stateMachineWasStarted = signal.WaitOne(500);
            stateMachineWasStarted.Should().BeTrue();

            testee.Stop();

            enteredB
                .Should()
                .BeTrue();
        }

        [Fact]
        public void PassiveStateMachineThrowsExceptionOnLoading_WhenAlreadyStarted()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder.In(States.A);
            var testee = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build()
                .CreatePassiveStateMachine();

            testee.Start();

            Action action = () => testee.Load(A.Fake<IStateMachineLoader<States, Events>>());

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.StateMachineIsAlreadyInitialized);
        }

        [Fact]
        public void ThrowsExceptionOnLoading_WhenAlreadyStarted()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder.In(States.A);
            var testee = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build()
                .CreateActiveStateMachine();

            var signal = new AutoResetEvent(false);
            var extension = A.Fake<IExtension<States, Events>>();
            A.CallTo(() =>
                    extension.EnteredInitialState(
                        A<IStateMachineInformation<States, Events>>.Ignored,
                        A<States>.Ignored,
                        A<ITransitionContext<States, Events>>.Ignored))
                .Invokes(() => signal.Set());
            testee.AddExtension(extension);

            testee.Start();

            var stateMachineWasStarted = signal.WaitOne(500);
            stateMachineWasStarted.Should().BeTrue();

            Action action = () => testee.Load(A.Fake<IStateMachineLoader<States, Events>>());

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.StateMachineIsAlreadyInitialized);
        }

        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void ThrowsExceptionOnLoading_WhenSettingALastActiveStateThatIsNotASubState(string dummyName, Func<StateMachineDefinition<States, Events>, IStateMachine<States, Events>> createStateMachine)
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .In(States.A);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.B)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(States.B1)
                    .WithSubState(States.B2);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var testee = createStateMachine(stateMachineDefinition);

            var loader = A.Fake<IStateMachineLoader<States, Events>>();

            A.CallTo(() => loader.LoadHistoryStates())
                .Returns(new Dictionary<States, States>
                            {
                                { States.B, States.A }
                            });

            Action action = () => testee.Load(loader);

            action.Should().Throw<InvalidOperationException>()
               .WithMessage(ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
        }

        /// <summary>
        /// Sets up the wait event that all transitions were completed.
        /// </summary>
        /// <param name="testee">The state machine to test.</param>
        /// <param name="numberOfTransitionCompletedMessages">The number of transition completed messages.</param>
        /// <returns>Event that is signaled when <paramref name="numberOfTransitionCompletedMessages"/> transition completed messages were received.</returns>
        private static AutoResetEvent SetUpWaitForAllTransitions(IStateMachine<States, Events> testee, int numberOfTransitionCompletedMessages)
        {
            var numberOfTransitionCompletedMessagesReceived = 0;
            var allTransitionsCompleted = new AutoResetEvent(false);
            testee.TransitionCompleted += (sender, e) =>
            {
                numberOfTransitionCompletedMessagesReceived++;
                if (numberOfTransitionCompletedMessagesReceived == numberOfTransitionCompletedMessages)
                {
                    allTransitionsCompleted.Set();
                }
            };

            return allTransitionsCompleted;
        }

        /// <summary>
        /// Waits for the event that was set-up with <see cref="SetUpWaitForAllTransitions"/> that all transitions were completed.
        /// </summary>
        /// <param name="allTransitionsCompleted">All transitions completed.</param>
        private static void WaitForAllTransitions(AutoResetEvent allTransitionsCompleted)
        {
            allTransitionsCompleted.WaitOne(1000)
                .Should().BeTrue("not enough transition completed events received within time-out.");
        }
    }
}