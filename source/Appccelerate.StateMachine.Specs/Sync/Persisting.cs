//-------------------------------------------------------------------------------
// <copyright file="Persisting.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Sync
{
    using System;
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Extensions;
    using Appccelerate.StateMachine.Infrastructure;
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Persistence;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;

    public class Persisting
    {
        public enum State
        {
            A, B, S, S1, S2
        }

        public enum Event
        {
            B, X, S2, S
        }

        [Scenario]
        public void Loading(
            StateMachineSaver<State> saver,
            StateMachineLoader<State> loader,
            FakeExtension extension,
            State sourceState,
            State targetState)
        {
            "establish a saved state machine with history".x(() =>
                {
                    var machine = new PassiveStateMachine<State, Event>();

                    DefineMachine(machine);
                    machine.Initialize(State.A);
                    machine.Start();
                    machine.Fire(Event.S2); // set history of super state S
                    machine.Fire(Event.B);  // set current state to B

                    saver = new StateMachineSaver<State>();
                    loader = new StateMachineLoader<State>();

                    machine.Save(saver);
                });

            "when state machine is loaded".x(() =>
                {
                    loader.SetCurrentState(saver.CurrentStateId);
                    loader.SetHistoryStates(saver.HistoryStates);

                    extension = new FakeExtension();
                    var loadedMachine = new PassiveStateMachine<State, Event>();
                    loadedMachine.AddExtension(extension);

                    DefineMachine(loadedMachine);
                    loadedMachine.Load(loader);

                    loadedMachine.TransitionCompleted += (sender, args) =>
                        {
                            sourceState = args.StateId;
                            targetState = args.NewStateId;
                        };

                    loadedMachine.Start();
                    loadedMachine.Fire(Event.S);
                });

            "it should reset current state".x(() =>
                sourceState.Should().Be(State.B));

            "it should reset all history states of super states".x(() =>
                targetState.Should().Be(State.S2));

            "it should notify extensions".x(()
                => extension.LoadedCurrentState
                    .Should().BeEquivalentTo(State.B));
        }

        [Scenario]
        public void LoadingNonInitializedStateMachine(
            PassiveStateMachine<State, Event> loadedMachine)
        {
            "when a non-initialized state machine is loaded".x(() =>
            {
                var loader = new StateMachineLoader<State>();
                loader.SetCurrentState(new Initializable<State>());
                loader.SetHistoryStates(new Dictionary<State, State>());

                loadedMachine = new PassiveStateMachine<State, Event>();
                DefineMachine(loadedMachine);
                loadedMachine.Load(loader);
            });

            "it should not be initialized already".x(() =>
            {
                Action act = () => loadedMachine.Initialize(State.S);
                act.Should().NotThrow();
            });
        }

        [Scenario]
        public void LoadingAnInitializedStateMachine(
            PassiveStateMachine<string, int> machine,
            Exception receivedException)
        {
            "establish an initialized state machine".x(() =>
                {
                    machine = new PassiveStateMachine<string, int>();
                    machine.Initialize("initial");
                });

            "when state machine is loaded".x(() =>
                {
                    receivedException = Catch.Exception(() => machine.Load(A.Fake<IStateMachineLoader<string>>()));
                });

            "it should throw invalid operation exception".x(() =>
                {
                    receivedException.Should().BeOfType<InvalidOperationException>();
                    receivedException.Message.Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
                });
        }

        private static void DefineMachine(IStateMachine<State, Event> fsm)
        {
            fsm.In(State.A)
                   .On(Event.S2).Goto(State.S2)
                   .On(Event.X);

            fsm.In(State.B)
                .On(Event.S).Goto(State.S)
                .On(Event.X);

            fsm.DefineHierarchyOn(State.S)
               .WithHistoryType(HistoryType.Deep)
               .WithInitialSubState(State.S1)
               .WithSubState(State.S2);

            fsm.In(State.S)
                .On(Event.B).Goto(State.B);
        }

        public class FakeExtension : ExtensionBase<State, Event>
        {
            public List<State> LoadedCurrentState { get; } = new List<State>();

            public override void Loaded(
                IStateMachineInformation<State, Event> stateMachineInformation,
                Initializable<State> loadedCurrentState,
                IReadOnlyDictionary<State, State> loadedHistoryStates)
            {
                this.LoadedCurrentState.Add(loadedCurrentState.Value);
            }
        }

        public class StateMachineSaver<TState> : IStateMachineSaver<TState>
            where TState : IComparable
        {
            public Initializable<TState> CurrentStateId { get; private set; }

            public IReadOnlyDictionary<TState, TState> HistoryStates { get; private set; }

            public void SaveCurrentState(Initializable<TState> currentState)
            {
                this.CurrentStateId = currentState;
            }

            public void SaveHistoryStates(IReadOnlyDictionary<TState, TState> historyStates)
            {
                this.HistoryStates = historyStates;
            }
        }

        public class StateMachineLoader<TState> : IStateMachineLoader<TState>
            where TState : IComparable
        {
            private Initializable<TState> currentState;
            private IReadOnlyDictionary<TState, TState> historyStates;

            public void SetCurrentState(Initializable<TState> state)
            {
                this.currentState = state;
            }

            public void SetHistoryStates(IReadOnlyDictionary<TState, TState> states)
            {
                this.historyStates = states;
            }

            public IReadOnlyDictionary<TState, TState> LoadHistoryStates()
            {
                return this.historyStates;
            }

            public Initializable<TState> LoadCurrentState()
            {
                return this.currentState;
            }
        }
    }
}