//-------------------------------------------------------------------------------
// <copyright file="HierarchicalStateMachineInitialization.cs" company="Appccelerate">
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
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class HierarchicalStateMachineInitialization
    {
        private const int LeafState = 1;
        private const int SuperState = 0;

        [Scenario]
        public void InitializationInLeafState(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension testExtension,
            bool entryActionOfLeafStateExecuted,
            bool entryActionOfSuperStateExecuted)
        {
            "establish a hierarchical state machine with leaf state as initial state".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(SuperState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(LeafState);
                stateMachineDefinitionBuilder
                    .In(SuperState)
                    .ExecuteOnEntry(() => entryActionOfSuperStateExecuted = true);
                stateMachineDefinitionBuilder
                    .In(LeafState)
                    .ExecuteOnEntry(() => entryActionOfLeafStateExecuted = true);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(LeafState)
                    .Build()
                    .CreatePassiveStateMachine();

                testExtension = new CurrentStateExtension();
                machine.AddExtension(testExtension);
            });

            "when starting the state machine".x(async () =>
                await machine.Start());

            "it should set current state of state machine to state to which it is initialized".x(() =>
                testExtension.CurrentState
                    .Should().Be(LeafState));

            "it should execute entry action of state to which state machine is initialized".x(() =>
                entryActionOfLeafStateExecuted
                    .Should().BeTrue());

            "it should execute entry action of super states of the state to which state machine is initialized".x(() =>
                entryActionOfSuperStateExecuted
                    .Should().BeTrue());
        }

        [Scenario]
        public void InitializationInSuperState(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension testExtension,
            bool entryActionOfLeafStateExecuted,
            bool entryActionOfSuperStateExecuted)
        {
            "establish a hierarchical state machine with super state as initial state".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(SuperState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(LeafState);
                stateMachineDefinitionBuilder
                    .In(SuperState)
                    .ExecuteOnEntry(() => entryActionOfSuperStateExecuted = true);
                stateMachineDefinitionBuilder
                    .In(LeafState)
                    .ExecuteOnEntry(() => entryActionOfLeafStateExecuted = true);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SuperState)
                    .Build()
                    .CreatePassiveStateMachine();

                testExtension = new CurrentStateExtension();
                machine.AddExtension(testExtension);
            });

            "when starting the state machine".x(async () =>
                await machine.Start());

            "it should_set_current_state_of_state_machine_to_initial_leaf_state_of_the_state_to_which_it_is_initialized".x(() =>
                testExtension.CurrentState
                    .Should().Be(LeafState));

            "it should_execute_entry_action_of_super_state_to_which_state_machine_is_initialized".x(() =>
                entryActionOfSuperStateExecuted
                    .Should().BeTrue());

            "it should_execute_entry_actions_of_initial_sub_states_until_a_leaf_state_is_reached".x(() =>
                entryActionOfLeafStateExecuted
                    .Should().BeTrue());
        }
    }
}