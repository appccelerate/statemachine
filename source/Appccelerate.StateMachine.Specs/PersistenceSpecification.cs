//-------------------------------------------------------------------------------
// <copyright file="PersistenceSpecification.cs" company="Appccelerate">
//   Copyright (c) 2008-2013
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

namespace Appccelerate.StateMachine
{
    using System;
    using System.Collections.Generic;
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Persistence;

    using FakeItEasy;

    using FluentAssertions;
    using global::Machine.Specifications;

    [Subject(Concern.Persistence)]
    public class When_resetting_a_state_machine_from_persisted_data
    {
        static IStateMachine<State, Event> machine;
        static PassiveStateMachine<State, Event> loadedMachine;
        static StateMachineSaver<State> saver;
        static StateMachineLoader<State> loader;
        static State sourceState;
        static State targetState;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<State, Event>();
                DefineMachine(machine);
                machine.Initialize(State.A);
                machine.Start();
                machine.Fire(Event.S2); // set history of super state S
                machine.Fire(Event.B);  // set current state to B

                saver = new StateMachineSaver<State>();
                loader = new StateMachineLoader<State>();
            };

        Because of = () =>
            {
                machine.Save(saver);
                loader.SetCurrentState(saver.CurrentStateId);
                loader.SetHistoryStates(saver.HistoryStates);
                
                loadedMachine = new PassiveStateMachine<State, Event>();
                DefineMachine(loadedMachine);
                loadedMachine.Load(loader);

                loadedMachine.TransitionCompleted += (sender, args) =>
                    {
                        sourceState = args.StateId;
                        targetState = args.NewStateId;
                    };
                
                loadedMachine.Start();
                loadedMachine.Fire(Event.S);
            };

        It should_reset_current_state = () =>
            sourceState.Should().Be(State.B);

        It should_reset_all_history_states_of_super_states = () =>
            targetState.Should().Be(State.S2);

        enum State
        {
            A, B, S, S1, S2
        }

        enum Event
        {
            B, X, S2, S
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
    }

    public class StateMachineSaver<TState> : IStateMachineSaver<TState>
        where TState : IComparable
    {
        public Initializable<TState> CurrentStateId { get; private set; }
        
        public IDictionary<TState, TState> HistoryStates { get; private set; }

        public void SaveCurrentState(Initializable<TState> currentState)
        {
            this.CurrentStateId = currentState;
        }

        public void SaveHistoryStates(IDictionary<TState, TState> historyStates)
        {
            this.HistoryStates = historyStates;
        }
    }

    public class StateMachineLoader<TState> : IStateMachineLoader<TState>
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

        public IDictionary<TState, TState> LoadHistoryStates()
        {
            return this.historyStates;
        }

        public Initializable<TState> LoadCurrentState()
        {
            return this.currentState;
        }
    }

    [Subject(Concern.Persistence)]
    public class When_loading_an_already_initialized_state_machine
    {
        static PassiveStateMachine<string, int> machine;
        static Exception receivedException;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<string, int>();
                machine.Initialize("initial");
            };

        Because of = () =>
            {
                receivedException = Catch.Exception(() => machine.Load(A.Fake<IStateMachineLoader<string>>()));
            };

        It should_throw_invalid_operation_exception = () =>
            {
                receivedException.Should().BeOfType<InvalidOperationException>();
                receivedException.Message.Should().Be(ExceptionMessages.StateMachineIsAlreadyInitialized);
            };
    }
}