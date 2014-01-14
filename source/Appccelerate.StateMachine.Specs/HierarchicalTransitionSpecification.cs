//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitionSpecification.cs" company="Appccelerate">
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
    using System.Globalization;

    using FluentAssertions;
    using global::Machine.Specifications;

    [Subject(Concern.Transition)]
    public class When_firing_an_event_onto_a_started_hierarchical_state_machine_with_source_and_destination_having_different_parents
    {
        const int SourceState = 1;
        const int ParentOfSourceState = 2;
        const int SiblingOfSourceState = 3;
        const int DestinationState = 4;
        const int ParentOfDestinationState = 5;
        const int SiblingOfDestinationState = 6;
        const int Event = 0;

        static PassiveStateMachine<int, int> machine;

        static string log = string.Empty;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.DefineHierarchyOn(ParentOfSourceState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(SourceState)
                    .WithSubState(SiblingOfSourceState);
                
                machine.DefineHierarchyOn(ParentOfDestinationState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(DestinationState)
                    .WithSubState(SiblingOfDestinationState);

                machine.In(SourceState)
                    .ExecuteOnExit(() => log += "exit" + SourceState)
                    .On(Event).Goto(DestinationState);

                machine.In(ParentOfSourceState)
                    .ExecuteOnExit(() => log += "exit" + ParentOfSourceState);

                machine.In(DestinationState)
                    .ExecuteOnEntry(() => log += "enter" + DestinationState);

                machine.In(ParentOfDestinationState)
                    .ExecuteOnEntry(() => log += "enter" + ParentOfDestinationState);

                machine.Initialize(SourceState);
                machine.Start();
            };

        Because of = () => machine.Fire(Event);

        It should_execute_exit_action_of_source_state = () => 
            log.Should().Contain("exit" + SourceState);

        It should_execute_exit_action_of_parent_of_source_state = () => 
            log.Should().Contain("exit" + ParentOfSourceState);

        It should_execute_entry_action_of_parent_of_destination_state = () => 
            log.Should().Contain("enter" + ParentOfDestinationState);

        It should_execute_entry_action_of_destination_state = () => 
            log.Should().Contain("enter" + DestinationState);

        It should_execute_actions_from_source_upwards_and_then_downwards_to_destination_state = () =>
            {
                int s = log.IndexOf(SourceState.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                int ps = log.IndexOf(ParentOfSourceState.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                int d = log.IndexOf(DestinationState.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                int pd = log.IndexOf(ParentOfDestinationState.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

                s.Should().BeLessThan(ps);
                ps.Should().BeLessThan(pd);
                pd.Should().BeLessThan(d);
            };
    }

    [Subject(Concern.Transition)]
    public class When_firing_an_event_onto_a_started_hierarchical_state_machine_with_source_and_destination_having_a_common_ancestor
    {
        const int CommonAncestorState = 0;
        const int SourceState = 1;
        const int ParentOfSourceState = 2;
        const int SiblingOfSourceState = 3;
        const int DestinationState = 4;
        const int ParentOfDestinationState = 5;
        const int SiblingOfDestinationState = 6;
        const int Event = 0;

        static PassiveStateMachine<int, int> machine;

        static bool commonAncestorStateLeft;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();

            machine.DefineHierarchyOn(CommonAncestorState)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(ParentOfSourceState)
                .WithSubState(ParentOfDestinationState);

            machine.DefineHierarchyOn(ParentOfSourceState)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(SourceState)
                .WithSubState(SiblingOfSourceState);

            machine.DefineHierarchyOn(ParentOfDestinationState)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(DestinationState)
                .WithSubState(SiblingOfDestinationState);

            machine.In(SourceState)
                .On(Event).Goto(DestinationState);

            machine.In(CommonAncestorState)
                .ExecuteOnExit(() => commonAncestorStateLeft = true);

            machine.Initialize(SourceState);
            machine.Start();
        };

        Because of = () => 
            machine.Fire(Event);

        It should_remain_inside_common_ancestor_state = () => 
            commonAncestorStateLeft.Should().BeFalse();
    }
}