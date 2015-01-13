//-------------------------------------------------------------------------------
// <copyright file="HierarchicalStateMachineInitialization.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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
    using FluentAssertions;
    using Xbehave;

    public class HierarchicalStateMachineInitialization
    {
        private const int LeafState = 1;
        private const int SuperState = 0;

        private CurrentStateExtension testExtension;
        private PassiveStateMachine<int, int> machine;

        private bool entryActionOfLeafStateExecuted;
        private bool entryActionOfSuperStateExecuted;

        [Background]
        public void Background()
        {
            "establish a hierarchical state machine"._(() =>
                {
                    testExtension = new CurrentStateExtension();

                    machine = new PassiveStateMachine<int, int>();

                    machine.AddExtension(testExtension);

                    machine.DefineHierarchyOn(SuperState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(LeafState);

                    machine.In(SuperState)
                        .ExecuteOnEntry(() => entryActionOfSuperStateExecuted = true);
                    machine.In(LeafState)
                        .ExecuteOnEntry(() => entryActionOfLeafStateExecuted = true);
                });
        }

        [Scenario]
        public void InitializationInLeafState()
        {
            "when initializing to a leaf state and starting the state machine"._(() =>
            {
                machine.Initialize(LeafState);
                machine.Start();
            });

            "it should set current state of state machine to state to which it is initialized"._(() => 
                this.testExtension.CurrentState
                    .Should().Be(LeafState));

            "it should execute entry action of state to which state machine is initialized"._(() => 
                this.entryActionOfLeafStateExecuted
                    .Should().BeTrue());

            "it should execute entry action of super states of the state to which state machine is initialized"._(() => 
                this.entryActionOfSuperStateExecuted
                    .Should().BeTrue());
        }

        [Scenario]
        public void InitializationInSuperState()
        {
            "when initializing to a super state and starting the state machine"._(() =>
            {
                machine.Initialize(SuperState);
                machine.Start();
            });

            "it should_set_current_state_of_state_machine_to_initial_leaf_state_of_the_state_to_which_it_is_initialized"._(() =>
                this.testExtension.CurrentState
                    .Should().Be(LeafState));

            "it should_execute_entry_action_of_super_state_to_which_state_machine_is_initialized"._(() =>
                this.entryActionOfSuperStateExecuted
                    .Should().BeTrue());

            "it should_execute_entry_actions_of_initial_sub_states_until_a_leaf_state_is_reached"._(() =>
                this.entryActionOfLeafStateExecuted
                    .Should().BeTrue());           
        }
    }
}