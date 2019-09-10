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
            StateMachineSaver<State> saver,
            StateMachineLoader<State> loader,
            FakeExtension extension,
            State sourceState,
            State targetState)
        {
            "establish a saved state machine with history".x(async () =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<State, Event>();
                SetupStates(stateMachineDefinitionBuilder);
                var machine = stateMachineDefinitionBuilder
                    .Build()
                    .CreatePassiveStateMachine();

                await machine.Initialize(State.A);
                await machine.Start();
                await machine.Fire(Event.S2); // set history of super state S
                await machine.Fire(Event.B);  // set current state to B

                saver = new StateMachineSaver<State>();
                loader = new StateMachineLoader<State>();

                await machine.Save(saver);
            });

            "when state machine is loaded".x(async () =>
            {
                loader.SetCurrentState(saver.CurrentStateId);
                loader.SetHistoryStates(saver.HistoryStates);

                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<State, Event>();
                SetupStates(stateMachineDefinitionBuilder);
                var loadedMachine = stateMachineDefinitionBuilder
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
                => extension.LoadedCurrentState
                    .Should().BeEquivalentTo(State.B));
        }

        [Scenario]
        public void LoadingNonInitializedStateMachine(
            AsyncPassiveStateMachine<State, Event> loadedMachine)
        {
            "when a non-initialized state machine is loaded".x(async () =>
            {
                var loader = new StateMachineLoader<State>();
                loader.SetCurrentState(new Initializable<State>());
                loader.SetHistoryStates(new Dictionary<State, State>());

                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<State, Event>();
                SetupStates(stateMachineDefinitionBuilder);
                loadedMachine = stateMachineDefinitionBuilder
                    .Build()
                    .CreatePassiveStateMachine();
                await loadedMachine.Load(loader);
            });

            "it should not be initialized already".x(() =>
            {
                Func<Task> a = async () =>
                    await loadedMachine.Initialize(State.S);
                a.Should().NotThrow();
            });
        }

        [Scenario]
        public void LoadingAnInitializedStateMachine(
            IAsyncStateMachineNew<string, int> machine,
            Exception receivedException)
        {
            "establish an initialized state machine".x(() =>
            {
                machine = new StateMachineDefinitionBuilder<string, int>()
                    .Build()
                    .CreatePassiveStateMachine();
                machine.Initialize("initial");
            });

            "when state machine is loaded".x(async () =>
            {
                receivedException = await Catch.Exception(async () => await machine.Load(A.Fake<IAsyncStateMachineLoader<string>>()));
            });

            "it should throw invalid operation exception".x(() =>
            {
                receivedException.Should().BeOfType<InvalidOperationException>();
                receivedException.Message.Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
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

        public class FakeExtension : AsyncExtensionBase<State, Event>
        {
            public List<State> LoadedCurrentState { get; } = new List<State>();

            public override void Loaded(
                IStateMachineInformation<State, Event> stateMachineInformation,
                Initializable<State> loadedCurrentState,
                IDictionary<State, State> loadedHistoryStates)
            {
                this.LoadedCurrentState.Add(loadedCurrentState.Value);
            }
        }

        public class StateMachineSaver<TState> : IAsyncStateMachineSaver<TState>
            where TState : IComparable
        {
            public Initializable<TState> CurrentStateId { get; private set; }

            public IDictionary<TState, TState> HistoryStates { get; private set; }

            public Task SaveCurrentState(Initializable<TState> currentState)
            {
                this.CurrentStateId = currentState;

                return Task.CompletedTask;
            }

            public Task SaveHistoryStates(IDictionary<TState, TState> historyStates)
            {
                this.HistoryStates = historyStates;

                return Task.CompletedTask;
            }
        }

        public class StateMachineLoader<TState> : IAsyncStateMachineLoader<TState>
            where TState : IComparable
        {
            private Initializable<TState> currentState;
            private IDictionary<TState, TState> historyStates;

            public void SetCurrentState(Initializable<TState> state)
            {
                this.currentState = state;
            }

            public void SetHistoryStates(IDictionary<TState, TState> states)
            {
                this.historyStates = states;
            }

            public Task<IDictionary<TState, TState>> LoadHistoryStates()
            {
                return Task.FromResult(this.historyStates);
            }

            public Task<Initializable<TState>> LoadCurrentState()
            {
                return Task.FromResult(this.currentState);
            }
        }
    }
}