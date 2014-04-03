//-------------------------------------------------------------------------------
// <copyright file="TransitionSpecification.cs" company="Appccelerate">
//   Copyright (c) 2008-2014
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

    [Subject(Concern.Transition)]
    public class When_firing_an_event_onto_a_started_state_machine
    {
        const int SourceState = 1;
        const int DestinationState = 2;
        const int Event = 2;

        const string Parameter = "parameter";

        static string actualParameter;
        static bool exitActionExecuted;
        static bool entryActionExecuted;

        static PassiveStateMachine<int, int> machine;

        static CurrentStateExtension currentStateExtension;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<int, int>();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                machine.In(SourceState)
                    .ExecuteOnExit(() => exitActionExecuted = true)
                    .On(Event).Goto(DestinationState).Execute<string>(p => actualParameter = p);

                machine.In(DestinationState)
                    .ExecuteOnEntry(() => entryActionExecuted = true);

                machine.Initialize(SourceState);
                machine.Start();
            };

        Because of = () =>
            {
                machine.Fire(Event, Parameter);
            };

        It should_execute_transition_by_switching_state = () =>
            {
                currentStateExtension.CurrentState.Should().Be(DestinationState);
            };

        It should_execute_transition_actions = () =>
            {
                actualParameter.Should().NotBeNull();
            };

        It should_pass_parameters_to_transition_action = () =>
            {
                actualParameter.Should().Be(Parameter);
            };

        It should_execute_exit_action_of_source_state = () =>
            {
                exitActionExecuted.Should().BeTrue();
            };

        It should_execute_entry_action_of_destination_state = () =>
        {
            entryActionExecuted.Should().BeTrue();
        };
    }
}