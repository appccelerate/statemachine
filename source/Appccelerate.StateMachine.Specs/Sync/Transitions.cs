//-------------------------------------------------------------------------------
// <copyright file="Transitions.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Sync
{
    using FluentAssertions;
    using Machine;
    using Xbehave;

    public class Transitions
    {
        private const int SourceState = 1;
        private const int DestinationState = 2;
        private const int Event = 2;

        private const string Parameter = "parameter";

        private static readonly CurrentStateExtension CurrentStateExtension = new CurrentStateExtension();

        [Scenario]
        public void ExecutingTransition(
            PassiveStateMachine<int, int> machine,
            string actualParameter,
            bool exitActionExecuted,
            bool entryActionExecuted)
        {
            "establish a state machine with transitions".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(SourceState)
                        .ExecuteOnExit(() => exitActionExecuted = true)
                        .On(Event)
                        .Goto(DestinationState)
                        .Execute<string>(p => actualParameter = p);
                stateMachineDefinitionBuilder
                    .In(DestinationState)
                        .ExecuteOnEntry(() => entryActionExecuted = true);
                machine = stateMachineDefinitionBuilder
                    .WithInitialState(SourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                machine.AddExtension(CurrentStateExtension);

                machine.Start();
            });

            "when firing an event onto the state machine".x(() =>
                machine.Fire(Event, Parameter));

            "it should_execute_transition_by_switching_state".x(() =>
                 CurrentStateExtension.CurrentState.Should().Be(DestinationState));

            "it should_execute_transition_actions".x(() =>
                 actualParameter.Should().NotBeNull());

            "it should_pass_parameters_to_transition_action".x(() =>
                 actualParameter.Should().Be(Parameter));

            "it should_execute_exit_action_of_source_state".x(() =>
                 exitActionExecuted.Should().BeTrue());

            "it should_execute_entry_action_of_destination_state".x(() =>
                entryActionExecuted.Should().BeTrue());
        }
    }
}