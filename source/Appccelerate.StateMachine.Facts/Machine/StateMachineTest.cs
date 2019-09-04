//-------------------------------------------------------------------------------
// <copyright file="StateMachineTest.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using FluentAssertions;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using Xunit;

    /// <summary>
    /// Tests state machine initialization and state switching.
    /// </summary>
    public class StateMachineTest
    {
        private readonly IReadOnlyDictionary<StateMachine.States, StateDefinition<StateMachine.States, Events>> stateDefinitions;

        /// <summary>
        /// The list of recorded actions.
        /// </summary>
        private readonly List<Record> records = new List<Record>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineTest"/> class.
        /// </summary>
        public StateMachineTest()
        {
            this.stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.DefineHierarchyOn(StateMachine.States.B)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(StateMachine.States.B1)
                        .WithSubState(StateMachine.States.B2))
                .WithConfiguration(x =>
                    x.DefineHierarchyOn(StateMachine.States.C)
                        .WithHistoryType(HistoryType.Shallow)
                        .WithInitialSubState(StateMachine.States.C2)
                        .WithSubState(StateMachine.States.C1))
                .WithConfiguration(x =>
                    x.DefineHierarchyOn(StateMachine.States.C1)
                        .WithHistoryType(HistoryType.Shallow)
                        .WithInitialSubState(StateMachine.States.C1A)
                        .WithSubState(StateMachine.States.C1B))
                .WithConfiguration(x =>
                    x.DefineHierarchyOn(StateMachine.States.D)
                        .WithHistoryType(HistoryType.Deep)
                        .WithInitialSubState(StateMachine.States.D1)
                        .WithSubState(StateMachine.States.D2))
                .WithConfiguration(x =>
                    x.DefineHierarchyOn(StateMachine.States.D1)
                        .WithHistoryType(HistoryType.Deep)
                        .WithInitialSubState(StateMachine.States.D1A)
                        .WithSubState(StateMachine.States.D1B))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.A))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.A))
                        .On(Events.B).Goto(StateMachine.States.B)
                        .On(Events.C).Goto(StateMachine.States.C)
                        .On(Events.D).Goto(StateMachine.States.D)
                        .On(Events.A))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.B)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.B))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.B))
                        .On(Events.D).Goto(StateMachine.States.D))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.B1)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.B1))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.B1))
                        .On(Events.B2).Goto(StateMachine.States.B2))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.B2)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.B2))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.B2))
                        .On(Events.A).Goto(StateMachine.States.A)
                        .On(Events.C1B).Goto(StateMachine.States.C1B))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.C)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.C))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.C))
                        .On(Events.A).Goto(StateMachine.States.A))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.C1)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.C1))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.C1))
                        .On(Events.C1B).Goto(StateMachine.States.C1B))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.C2)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.C2))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.C2)))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.C1A)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.C1A))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.C1A)))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.C1B)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.C1B))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.C1B)))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.D)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.D))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.D)))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.D1)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.D1))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.D1)))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.D1A)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.D1A))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.D1A)))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.D1B)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.D1B))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.D1B))
                        .On(Events.A).Goto(StateMachine.States.A)
                        .On(Events.B1).Goto(StateMachine.States.B1))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.D2)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.D2))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.D2))
                        .On(Events.A).Goto(StateMachine.States.A))
                .WithConfiguration(x =>
                    x.In(StateMachine.States.E)
                        .ExecuteOnEntry(() => this.RecordEntry(StateMachine.States.E))
                        .ExecuteOnExit(() => this.RecordExit(StateMachine.States.E))
                        .On(Events.A).Goto(StateMachine.States.A)
                        .On(Events.E).Goto(StateMachine.States.E))
                .Build();
        }

        [Fact]
        public void InitializationWhenInitialStateIsNotYetEnteredThenNoActionIsPerformed()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);

            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// After initialization the state machine is in the initial state and the initial state is entered.
        /// </summary>
        [Fact]
        public void InitializeToTopLevelState()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.A);

            this.CheckRecord<EntryRecord>(StateMachine.States.A);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// After initialization the state machine is in the initial state and the initial state is entered.
        /// All states up in the hierarchy of the initial state are entered, too.
        /// </summary>
        [Fact]
        public void InitializeToNestedState()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.D1B, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.D1B);

            this.CheckRecord<EntryRecord>(StateMachine.States.D);
            this.CheckRecord<EntryRecord>(StateMachine.States.D1);
            this.CheckRecord<EntryRecord>(StateMachine.States.D1B);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When the state machine is initializes to a state with sub-states then the hierarchy is recursively
        /// traversed to the most nested state along the chain of initial states.
        /// </summary>
        [Fact]
        public void InitializeStateWithSubStates()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            stateContainer.SetLastActiveStateFor(StateMachine.States.D, this.stateDefinitions[StateMachine.States.D1]);
            stateContainer.SetLastActiveStateFor(StateMachine.States.D1, this.stateDefinitions[StateMachine.States.D1A]);

            testee.Initialize(StateMachine.States.D, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.D1A);

            this.CheckRecord<EntryRecord>(StateMachine.States.D);
            this.CheckRecord<EntryRecord>(StateMachine.States.D1);
            this.CheckRecord<EntryRecord>(StateMachine.States.D1A);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition between two states at the top level then the
        /// exit action of the source state is executed, then the action is performed
        /// and the entry action of the target state is executed.
        /// Finally, the current state is the target state.
        /// </summary>
        [Fact]
        public void ExecuteTransition()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.E, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.A, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.A);

            this.CheckRecord<ExitRecord>(StateMachine.States.E);
            this.CheckRecord<EntryRecord>(StateMachine.States.A);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition between two states with the same super state is executed then
        /// the exit action of source state, the transition action and the entry action of
        /// the target state are executed.
        /// </summary>
        [Fact]
        public void ExecuteTransitionBetweenStatesWithSameSuperState()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.B1, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.B2, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B2);

            this.CheckRecord<ExitRecord>(StateMachine.States.B1);
            this.CheckRecord<EntryRecord>(StateMachine.States.B2);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition between two states in different super states on different levels is executed
        /// then all states from the source up to the common super-state are exited and all states down to
        /// the target state are entered. In this case the target state is lower than the source state.
        /// </summary>
        [Fact]
        public void ExecuteTransitionBetweenStatesOnDifferentLevelsDownwards()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.B2, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.C1B, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.C1B);

            this.CheckRecord<ExitRecord>(StateMachine.States.B2);
            this.CheckRecord<ExitRecord>(StateMachine.States.B);
            this.CheckRecord<EntryRecord>(StateMachine.States.C);
            this.CheckRecord<EntryRecord>(StateMachine.States.C1);
            this.CheckRecord<EntryRecord>(StateMachine.States.C1B);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition between two states in different super states on different levels is executed
        /// then all states from the source up to the common super-state are exited and all states down to
        /// the target state are entered. In this case the target state is higher than the source state.
        /// </summary>
        [Fact]
        public void ExecuteTransitionBetweenStatesOnDifferentLevelsUpwards()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.D1B, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.B1, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B1);

            this.CheckRecord<ExitRecord>(StateMachine.States.D1B);
            this.CheckRecord<ExitRecord>(StateMachine.States.D1);
            this.CheckRecord<ExitRecord>(StateMachine.States.D);
            this.CheckRecord<EntryRecord>(StateMachine.States.B);
            this.CheckRecord<EntryRecord>(StateMachine.States.B1);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition targets a super-state then the initial-state of this super-state is entered recursively
        /// down to the most nested state. No history here!
        /// </summary>
        [Fact]
        public void ExecuteTransitionWithInitialSubState()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.B, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B1);

            this.CheckRecord<ExitRecord>(StateMachine.States.A);
            this.CheckRecord<EntryRecord>(StateMachine.States.B);
            this.CheckRecord<EntryRecord>(StateMachine.States.B1);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition targets a super-state with <see cref="HistoryType.None"/> then the initial
        /// sub-state is entered whatever sub.state was last active.
        /// </summary>
        [Fact]
        public void ExecuteTransitionWithHistoryTypeNone()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.B2, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);
            testee.Fire(Events.A, stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.B, stateContainer, stateContainer, this.stateDefinitions);

            this.CheckRecord<ExitRecord>(StateMachine.States.A);
            this.CheckRecord<EntryRecord>(StateMachine.States.B);
            this.CheckRecord<EntryRecord>(StateMachine.States.B1);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition targets a super-state with <see cref="HistoryType.Shallow"/> then the last
        /// active sub-state is entered and the initial-state of the entered sub-state is entered (no recursive history).
        /// </summary>
        [Fact]
        public void ExecuteTransitionWithHistoryTypeShallow()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.C1B, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);
            testee.Fire(Events.A, stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.C, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.C1A);

            this.CheckRecord<ExitRecord>(StateMachine.States.A);
            this.CheckRecord<EntryRecord>(StateMachine.States.C);
            this.CheckRecord<EntryRecord>(StateMachine.States.C1);
            this.CheckRecord<EntryRecord>(StateMachine.States.C1A);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// When a transition targets a super-state with <see cref="HistoryType.Deep"/> then the last
        /// active sub-state is entered recursively down to the most nested state.
        /// </summary>
        [Fact]
        public void ExecuteTransitionWithHistoryTypeDeep()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.D1B, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);
            testee.Fire(Events.A, stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.D, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.D1B);

            this.CheckRecord<ExitRecord>(StateMachine.States.A);
            this.CheckRecord<EntryRecord>(StateMachine.States.D);
            this.CheckRecord<EntryRecord>(StateMachine.States.D1);
            this.CheckRecord<EntryRecord>(StateMachine.States.D1B);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// The state hierarchy is recursively walked up until a state can handle the event.
        /// </summary>
        [Fact]
        public void ExecuteTransitionHandledBySuperState()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.C1B, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);

            this.ClearRecords();

            testee.Fire(Events.A, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.A);

            this.CheckRecord<ExitRecord>(StateMachine.States.C1B);
            this.CheckRecord<ExitRecord>(StateMachine.States.C1);
            this.CheckRecord<ExitRecord>(StateMachine.States.C);
            this.CheckRecord<EntryRecord>(StateMachine.States.A);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// Internal transitions do not trigger any exit or entry actions and the state machine remains in the same state.
        /// </summary>
        [Fact]
        public void InternalTransition()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);
            this.ClearRecords();

            testee.Fire(Events.A, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.A);
        }

        [Fact]
        public void ExecuteSelfTransition()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.E, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);
            this.ClearRecords();

            testee.Fire(Events.E, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.E);

            this.CheckRecord<ExitRecord>(StateMachine.States.E);
            this.CheckRecord<EntryRecord>(StateMachine.States.E);
            this.CheckNoRemainingRecords();
        }

        [Fact]
        public void ExecuteTransitionToNephew()
        {
            var stateContainer = new StateContainer<StateMachine.States, Events>();
            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.C1A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, this.stateDefinitions);
            this.ClearRecords();

            testee.Fire(Events.C1B, stateContainer, stateContainer, this.stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.C1B);

            this.CheckRecord<ExitRecord>(StateMachine.States.C1A);
            this.CheckRecord<EntryRecord>(StateMachine.States.C1B);
            this.CheckNoRemainingRecords();
        }

        /// <summary>
        /// Records the entry into a state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void RecordEntry(StateMachine.States state)
        {
            this.records.Add(new EntryRecord { State = state });
        }

        /// <summary>
        /// Records the exit out of a state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void RecordExit(StateMachine.States state)
        {
            this.records.Add(new ExitRecord { State = state });
        }

        /// <summary>
        /// Clears the records.
        /// </summary>
        private void ClearRecords()
        {
            this.records.Clear();
        }

        /// <summary>
        /// Checks that the first record in the list of records is of type <typeparamref name="T"/> and involves the specified state.
        /// The record is removed after the check.
        /// </summary>
        /// <typeparam name="T">Type of the record.</typeparam>
        /// <param name="state">The state.</param>
        private void CheckRecord<T>(StateMachine.States state)
            where T : Record
        {
            Record record = this.records.FirstOrDefault();

            record.Should().NotBeNull();
            record.Should().BeAssignableTo<T>();
            //// ReSharper disable once PossibleNullReferenceException
            record.State.Should().Be(state, record.Message);

            this.records.RemoveAt(0);
        }

        /// <summary>
        /// Checks that no remaining records are present.
        /// </summary>
        private void CheckNoRemainingRecords()
        {
            if (this.records.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder("there are additional records:");
            foreach (var s in from record in this.records select record.GetType().Name + "-" + record.State)
            {
                sb.AppendLine();
                sb.Append(s);
            }

            this.records.Should().BeEmpty(sb.ToString());
        }

        /// <summary>
        /// A record of something that happened.
        /// </summary>
        [DebuggerDisplay("Message = {Message}")]
        private abstract class Record
        {
            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>The state.</value>
            public StateMachine.States State { get; set; }

            /// <summary>
            /// Gets the message.
            /// </summary>
            /// <value>The message.</value>
            public abstract string Message { get; }
        }

        /// <summary>
        /// Record of a state entry.
        /// </summary>
        private class EntryRecord : Record
        {
            /// <summary>
            /// Gets the message.
            /// </summary>
            /// <value>The message.</value>
            public override string Message => "State " + this.State + " not entered.";
        }

        /// <summary>
        /// Record of a state exit.
        /// </summary>
        private class ExitRecord : Record
        {
            /// <summary>
            /// Gets the message.
            /// </summary>
            /// <value>The message.</value>
            public override string Message => "State " + this.State + " not exited.";
        }
    }
}