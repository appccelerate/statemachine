//-------------------------------------------------------------------------------
// <copyright file="HierarchicalStateMachineInitializationSpecification.cs" company="Appccelerate">
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
    using FluentAssertions;
    using global::Machine.Specifications;

    [Subject(Concern.Initialization)]
    public class When_initializing_to_leaf_state_of_a_hierarchical_state_machine : HierarchicalStateMachineInitializationSpecification
    {
        Because of = () =>
        {
            machine.Initialize(LeafState);
            machine.Start();
        };

        It should_set_current_state_of_state_machine_to_state_to_which_it_is_initialized = () =>
        {
            testExtension.CurrentState.Should().Be(LeafState);
        };

        It should_execute_entry_action_of_state_to_which_state_machine_is_initialized = () =>
        {
            entryActionOfLeafStateExecuted.Should().BeTrue();
        };

        It should_execute_entry_action_of_super_states_of_the_state_to_which_state_machine_is_initialized = () =>
        {
            entryActionOfSuperStateExecuted.Should().BeTrue();
        };
    }

    [Subject(Concern.Initialization)]
    public class When_initializing_to_a_super_state_of_a_hierarchical_state_machine : HierarchicalStateMachineInitializationSpecification
    {
        Because of = () =>
        {
            machine.Initialize(SuperState);
            machine.Start();
        };

        It should_set_current_state_of_state_machine_to_initial_leaf_state_of_the_state_to_which_it_is_initialized = () =>
        {
            testExtension.CurrentState.Should().Be(LeafState);
        };

        It should_execute_entry_action_of_super_state_to_which_state_machine_is_initialized = () =>
        {
            entryActionOfSuperStateExecuted.Should().BeTrue();
        };

        It should_execute_entry_actions_of_initial_sub_states_until_a_leaf_state_is_reached = () =>
        {
            entryActionOfLeafStateExecuted.Should().BeTrue();
        };
    }

    [Subject(Concern.Initialization)]
    public class HierarchicalStateMachineInitializationSpecification
    {
        protected const int LeafState = 1;
        protected const int SuperState = 0;

        protected static PassiveStateMachine<int, int> machine;

        protected static bool entryActionOfLeafStateExecuted;
        protected static bool entryActionOfSuperStateExecuted;

        protected static CurrentStateExtension testExtension;

        Establish context = () =>
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
        };
    }
}