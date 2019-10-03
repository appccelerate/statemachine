//-------------------------------------------------------------------------------
// <copyright file="Persisting.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncMachine;
    using FakeItEasy;
    using FluentAssertions;
    using Infrastructure;
    using Persistence;
    using Specs;
    using Xbehave;
    using ExceptionMessages = Machine.ExceptionMessages;

    public class Persisting
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        public enum State
        {
            A, B, S, S1, S2
        }

        public enum Event
        {
            B, X, S2, S
        }
#pragma warning restore SA1602 // Enumeration items should be documented

        [Scenario]
        public void Loading(
            StateMachineSaver<State, Event> saver,
            StateMachineLoader<State, Event> loader,
            FakeExtension extension,
            State sourceState,
            State targetState)
        {
            "establish a saved state machine with history".x(async () =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<State, Event>();
                SetupStates(stateMachineDefinitionBuilder);
                var machine = stateMachineDefinitionBuilder
                    .WithInitialState(State.A)
                    .Build()
                    .CreatePassiveStateMachine();

                await machine.Start();
                await machine.Fire(Event.S2); // set history of super state S
                await machine.Fire(Event.B);  // set current state to B

                saver = new StateMachineSaver<State, Event>();
                loader = new StateMachineLoader<State, Event>();

                await machine.Save(saver);
            });

            "when state machine is loaded".x(async () =>
            {
                loader.SetCurrentState(saver.CurrentStateId);
                loader.SetHistoryStates(saver.HistoryStates);

                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<State, Event>();
                SetupStates(stateMachineDefinitionBuilder);
                var loadedMachine = stateMachineDefinitionBuilder
                    .WithInitialState(State.A)
                    .Build()
                    .CreatePassiveStateMachine();

                extension = new FakeExtension();
                loadedMachine.AddExtension(extension);

                await loadedMachine.Load(loader);

                loadedMachine.TransitionCompleted += (sender, args) =>
                    {
                        sourceState = args.StateId;
                        targetState = args.NewStateId;
                    };

                await loadedMachine.Start();
                await loadedMachine.Fire(Event.S);
            });

            "it should reset current state".x(() =>
                sourceState.Should().Be(State.B));

            "it should reset all history states of super states".x(() =>
                targetState.Should().Be(State.S2));

            "it should notify extensions".x(()
                => extension
                    .LoadedCurrentState
                    .Should()
                    .BeEquivalentTo(Initializable<State>.Initialized(State.B)));
        }

        [Scenario]
        public void LoadingNonInitializedStateMachine(
            AsyncPassiveStateMachine<State, Event> loadedMachine)
        {
            "when a non-initialized state machine is loaded".x(async () =>
            {
                var loader = new StateMachineLoader<State, Event>();
                loader.SetCurrentState(Initializable<State>.UnInitialized());
                loader.SetHistoryStates(new Dictionary<State, State>());

                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<State, Event>();
                SetupStates(stateMachineDefinitionBuilder);
                loadedMachine = stateMachineDefinitionBuilder
                    .WithInitialState(State.A)
                    .Build()
                    .CreatePassiveStateMachine();
                await loadedMachine.Load(loader);
            });

            "it should not be initialized already".x(async () =>
            {
                var stateMachineSaver = new StateMachineSaver<State, Event>();
                await loadedMachine.Save(stateMachineSaver);
                stateMachineSaver
                    .CurrentStateId
                    .IsInitialized
                    .Should()
                    .BeFalse();
            });
        }

        [Scenario]
        public void LoadingAnInitializedStateMachine(
            IAsyncStateMachine<string, int> machine,
            Exception receivedException)
        {
            "establish an started state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder.In("initial");
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("initial")
                    .Build()
                    .CreatePassiveStateMachine();
                machine.Start();
            });

            "when state machine is loaded".x(async () =>
                receivedException = await Catch.Exception(async () =>
                    await machine.Load(A.Fake<IAsyncStateMachineLoader<string, int>>())));

            "it should throw invalid operation exception".x(() =>
            {
                receivedException.Should().BeOfType<InvalidOperationException>();
                receivedException.Message.Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
            });
        }

        [Scenario]
        public void SavingEventsForPassiveStateMachine(
            AsyncPassiveStateMachine<string, int> machine,
            StateMachineSaver<string, int> saver)
        {
            "establish a state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder
                    .In("A")
                    .On(1)
                    .Goto("B");
                stateMachineDefinitionBuilder
                    .In("B")
                    .On(2)
                    .Goto("C");
                stateMachineDefinitionBuilder
                    .In("C")
                    .On(3)
                    .Goto("A");
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("A")
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when events are fired".x(async () =>
            {
                await machine.Fire(1);
                await machine.Fire(2);
                await machine.FirePriority(3);
                await machine.FirePriority(4);
            });

            "and it is saved".x(async () =>
            {
                saver = new StateMachineSaver<string, int>();
                await machine.Save(saver);
            });

            "it should save those events".x(() =>
            {
                saver
                    .Events
                    .Select(x => x.EventId)
                    .Should()
                    .HaveCount(2)
                    .And
                    .ContainInOrder(1, 2);
                saver
                    .PriorityEvents
                    .Select(x => x.EventId)
                    .Should()
                    .HaveCount(2)
                    .And
                    .ContainInOrder(4, 3);
            });
        }

        [Scenario]
        public void LoadingEventsForPassiveStateMachine(
            AsyncPassiveStateMachine<string, int> machine)
        {
            "establish a passive state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder
                    .In("A")
                    .On(1)
                    .Goto("B");
                stateMachineDefinitionBuilder
                    .In("B")
                    .On(2)
                    .Goto("C");
                stateMachineDefinitionBuilder
                    .In("C")
                    .On(3)
                    .Goto("A");
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("A")
                    .Build()
                    .CreatePassiveStateMachine();
            });

            "when it is loaded with Events".x(async () =>
            {
                var firstEvent = new EventInformation<int>(2, null);
                var secondEvent = new EventInformation<int>(3, null);
                var priorityEvent = new EventInformation<int>(1, null);
                var loader = new StateMachineLoader<string, int>();
                loader.SetEvents(new List<EventInformation<int>>
                {
                    firstEvent,
                    secondEvent,
                });
                loader.SetPriorityEvents(new List<EventInformation<int>>
                {
                    priorityEvent
                });

                var extension = A.Fake<IExtension<string, int>>();
                machine.AddExtension(extension);

                await machine.Load(loader);

                A.CallTo(() => extension.Loaded(
                        A<IStateMachineInformation<string, int>>.Ignored,
                        A<Initializable<string>>.Ignored,
                        A<IReadOnlyDictionary<string, string>>.Ignored,
                        A<IReadOnlyCollection<EventInformation<int>>>
                            .That
                            .Matches(c =>
                                c.Count == 2
                                && c.Contains(firstEvent)
                                && c.Contains(secondEvent)),
                        A<IReadOnlyCollection<EventInformation<int>>>
                            .That
                            .Matches(c =>
                                c.Count == 1
                                && c.Contains(priorityEvent))))
                    .MustHaveHappenedOnceExactly();
            });

            "it should process those events".x(async () =>
            {
                var transitionRecords = new List<TransitionRecord>();
                machine.TransitionCompleted += (sender, args) =>
                    transitionRecords.Add(
                        new TransitionRecord(args.EventId, args.StateId, args.NewStateId));

                await machine.Start();
                transitionRecords
                    .Should()
                    .HaveCount(3)
                    .And
                    .IsEquivalentInOrder(new List<TransitionRecord>
                    {
                        new TransitionRecord(1, "A", "B"),
                        new TransitionRecord(2, "B", "C"),
                        new TransitionRecord(3, "C", "A")
                    });
            });
        }

        [Scenario]
        public void SavingEventsForActiveStateMachine(
            AsyncActiveStateMachine<string, int> machine,
            StateMachineSaver<string, int> saver)
        {
            "establish a state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder
                    .In("A")
                    .On(1)
                    .Goto("B");
                stateMachineDefinitionBuilder
                    .In("B")
                    .On(2)
                    .Goto("C");
                stateMachineDefinitionBuilder
                    .In("C")
                    .On(3)
                    .Goto("A");
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("A")
                    .Build()
                    .CreateActiveStateMachine();
            });

            "when events are fired".x(async () =>
            {
                await machine.Fire(1);
                await machine.Fire(2);
                await machine.FirePriority(3);
                await machine.FirePriority(4);
            });

            "and it is saved".x(async () =>
            {
                saver = new StateMachineSaver<string, int>();
                await machine.Save(saver);
            });

            "it should save those events".x(() =>
            {
                saver
                    .Events
                    .Select(x => x.EventId)
                    .Should()
                    .HaveCount(2)
                    .And
                    .ContainInOrder(1, 2);
                saver
                    .PriorityEvents
                    .Select(x => x.EventId)
                    .Should()
                    .HaveCount(2)
                    .And
                    .ContainInOrder(4, 3);
            });
        }

        [Scenario]
        public void LoadingEventsForActiveStateMachine(
            AsyncActiveStateMachine<string, int> machine)
        {
            "establish a passive state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder
                    .In("A")
                    .On(1)
                    .Goto("B");
                stateMachineDefinitionBuilder
                    .In("B")
                    .On(2)
                    .Goto("C");
                stateMachineDefinitionBuilder
                    .In("C")
                    .On(3)
                    .Goto("A");
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("A")
                    .Build()
                    .CreateActiveStateMachine();
            });

            "when it is loaded with Events".x(async () =>
            {
                var firstEvent = new EventInformation<int>(2, null);
                var secondEvent = new EventInformation<int>(3, null);
                var priorityEvent = new EventInformation<int>(1, null);
                var loader = new StateMachineLoader<string, int>();
                loader.SetEvents(new List<EventInformation<int>>
                {
                    firstEvent,
                    secondEvent,
                });
                loader.SetPriorityEvents(new List<EventInformation<int>>
                {
                    priorityEvent
                });

                var extension = A.Fake<IExtension<string, int>>();
                machine.AddExtension(extension);

                await machine.Load(loader);

                A.CallTo(() => extension.Loaded(
                        A<IStateMachineInformation<string, int>>.Ignored,
                        A<Initializable<string>>.Ignored,
                        A<IReadOnlyDictionary<string, string>>.Ignored,
                        A<IReadOnlyCollection<EventInformation<int>>>
                            .That
                            .Matches(c =>
                                c.Count == 2
                                && c.Contains(firstEvent)
                                && c.Contains(secondEvent)),
                        A<IReadOnlyCollection<EventInformation<int>>>
                            .That
                            .Matches(c =>
                                c.Count == 1
                                && c.Contains(priorityEvent))))
                    .MustHaveHappenedOnceExactly();
            });

            "it should process those events".x(async () =>
            {
                var transitionRecords = new List<TransitionRecord>();
                machine.TransitionCompleted += (sender, args) =>
                    transitionRecords.Add(
                        new TransitionRecord(args.EventId, args.StateId, args.NewStateId));

                var signal = SetUpWaitForAllTransitions(machine, 3);
                await machine.Start();
                WaitForAllTransitions(signal);

                transitionRecords
                    .Should()
                    .HaveCount(3)
                    .And
                    .IsEquivalentInOrder(new List<TransitionRecord>
                    {
                        new TransitionRecord(1, "A", "B"),
                        new TransitionRecord(2, "B", "C"),
                        new TransitionRecord(3, "C", "A")
                    });
            });
        }

        private static void SetupStates(StateMachineDefinitionBuilder<State, Event> builder)
        {
            builder
                .In(State.A)
                   .On(Event.S2).Goto(State.S2)
                   .On(Event.X);

            builder
                .In(State.B)
                    .On(Event.S).Goto(State.S)
                    .On(Event.X);

            builder
                .DefineHierarchyOn(State.S)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(State.S1)
                    .WithSubState(State.S2);

            builder
                .In(State.S)
                    .On(Event.B).Goto(State.B);
        }

        private static AutoResetEvent SetUpWaitForAllTransitions<TState, TEvent>(IAsyncStateMachine<TState, TEvent> testee, int numberOfTransitionCompletedMessages)
            where TState : IComparable
            where TEvent : IComparable
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

        private static void WaitForAllTransitions(AutoResetEvent allTransitionsCompleted)
        {
            allTransitionsCompleted.WaitOne(1000)
                .Should().BeTrue("not enough transition completed events received within time-out.");
        }

        public class TransitionRecord
        {
            public int EventId { get; }

            public string Source { get; }

            public string Destination { get; }

            public TransitionRecord(int eventId, string source, string destination)
            {
                this.EventId = eventId;
                this.Source = source;
                this.Destination = destination;
            }
        }

        public class FakeExtension : AsyncExtensionBase<State, Event>
        {
            public List<IInitializable<State>> LoadedCurrentState { get; } = new List<IInitializable<State>>();

            public override Task Loaded(
                IStateMachineInformation<State, Event> stateMachineInformation,
                IInitializable<State> loadedCurrentState,
                IReadOnlyDictionary<State, State> loadedHistoryStates,
                IReadOnlyCollection<EventInformation<Event>> events,
                IReadOnlyCollection<EventInformation<Event>> priorityEvents)
            {
                this.LoadedCurrentState.Add(loadedCurrentState);
                return Task.CompletedTask;
            }
        }

        public class StateMachineSaver<TState, TEvent> : IAsyncStateMachineSaver<TState, TEvent>
            where TState : IComparable
            where TEvent : IComparable
        {
            public IInitializable<TState> CurrentStateId { get; private set; }

            public IReadOnlyDictionary<TState, TState> HistoryStates { get; private set; }

            public IReadOnlyCollection<EventInformation<TEvent>> Events { get; private set; }

            public IReadOnlyCollection<EventInformation<TEvent>> PriorityEvents { get; private set; }

            public Task SaveCurrentState(IInitializable<TState> currentState)
            {
                this.CurrentStateId = currentState;

                return Task.CompletedTask;
            }

            public Task SaveHistoryStates(IReadOnlyDictionary<TState, TState> historyStates)
            {
                this.HistoryStates = historyStates;

                return Task.CompletedTask;
            }

            public Task SaveEvents(IReadOnlyCollection<EventInformation<TEvent>> events)
            {
                this.Events = events;
                return Task.CompletedTask;
            }

            public Task SavePriorityEvents(IReadOnlyCollection<EventInformation<TEvent>> priorityEvents)
            {
                this.PriorityEvents = priorityEvents;
                return Task.CompletedTask;
            }
        }

        public class StateMachineLoader<TState, TEvent> : IAsyncStateMachineLoader<TState, TEvent>
            where TState : IComparable
            where TEvent : IComparable
        {
            private IInitializable<TState> currentState = Initializable<TState>.UnInitialized();
            private IReadOnlyDictionary<TState, TState> historyStates = new Dictionary<TState, TState>();
            private IReadOnlyCollection<EventInformation<TEvent>> events = new List<EventInformation<TEvent>>();
            private IReadOnlyCollection<EventInformation<TEvent>> priorityEvents = new List<EventInformation<TEvent>>();

            public void SetCurrentState(IInitializable<TState> state)
            {
                this.currentState = state;
            }

            public void SetHistoryStates(IReadOnlyDictionary<TState, TState> states)
            {
                this.historyStates = states;
            }

            public Task<IReadOnlyDictionary<TState, TState>> LoadHistoryStates()
            {
                return Task.FromResult(this.historyStates);
            }

            public Task<IInitializable<TState>> LoadCurrentState()
            {
                return Task.FromResult(this.currentState);
            }

            public Task<IReadOnlyCollection<EventInformation<TEvent>>> LoadEvents()
            {
                return Task.FromResult(this.events);
            }

            public Task<IReadOnlyCollection<EventInformation<TEvent>>> LoadPriorityEvents()
            {
                return Task.FromResult(this.priorityEvents);
            }

            public void SetEvents(IReadOnlyCollection<EventInformation<TEvent>> events)
            {
                this.events = events;
            }

            public void SetPriorityEvents(IReadOnlyCollection<EventInformation<TEvent>> priorityEvents)
            {
                this.priorityEvents = priorityEvents;
            }
        }
    }
}