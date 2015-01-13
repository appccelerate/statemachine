//-------------------------------------------------------------------------------
// <copyright file="TransitionSpecification.cs" company="Appccelerate">
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

    public class Transitions
    {
        const int SourceState = 1;
        const int DestinationState = 2;
        const int Event = 2;

        const string Parameter = "parameter";

        static readonly CurrentStateExtension CurrentStateExtension = new CurrentStateExtension();

        [Scenario]
        public void ExecutingTransition(
            PassiveStateMachine<int, int> machine,
            string actualParameter,
            bool exitActionExecuted,
            bool entryActionExecuted)
        {
            "establish a state machine with transitions"._(() =>
                {
                    machine = new PassiveStateMachine<int, int>();

                    machine.AddExtension(CurrentStateExtension);

                    machine.In(SourceState)
                        .ExecuteOnExit(() => exitActionExecuted = true)
                        .On(Event).Goto(DestinationState).Execute<string>(p => actualParameter = p);

                    machine.In(DestinationState)
                        .ExecuteOnEntry(() => entryActionExecuted = true);

                    machine.Initialize(SourceState);
                    machine.Start();
                });

            "when firing an event onto the state machine"._(() =>
                machine.Fire(Event, Parameter));

            "it should_execute_transition_by_switching_state"._(() =>
                 CurrentStateExtension.CurrentState.Should().Be(DestinationState));

            "it should_execute_transition_actions"._(() =>
                 actualParameter.Should().NotBeNull());

            "it should_pass_parameters_to_transition_action"._(() =>
                 actualParameter.Should().Be(Parameter));

            "it should_execute_exit_action_of_source_state"._(() =>
                 exitActionExecuted.Should().BeTrue());

            "it should_execute_entry_action_of_destination_state"._(() =>
                entryActionExecuted.Should().BeTrue());
        }
    }
}