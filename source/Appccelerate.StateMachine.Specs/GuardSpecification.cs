//-------------------------------------------------------------------------------
// <copyright file="GuardSpecification.cs" company="Appccelerate">
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
    public class When_firing_an_event_onto_a_started_state_machine_with_guarded_transitions
    {
        const int SourceState = 1;
        const int DestinationState = 2;
        const int ErrorState = 3;

        const int Event = 2;

        static PassiveStateMachine<int, int> machine;
        static CurrentStateExtension currentStateExtension;

        private Establish context = () =>
            {
                machine = new PassiveStateMachine<int, int>();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                machine.In(SourceState)
                    .On(Event)
                        .If(() => false).Goto(ErrorState)
                        .If(() => true).Goto(DestinationState)
                        .If(() => true).Goto(ErrorState)
                        .Otherwise().Goto(ErrorState);

                machine.Initialize(SourceState);
                machine.Start();
            };

        private Because of = () => 
            machine.Fire(Event);

        private It should_take_transition_guarded_with_first_matching_guard = () => 
            currentStateExtension.CurrentState.Should().Be(DestinationState);
    }

    [Subject(Concern.Transition)]
    public class When_firing_an_event_onto_a_started_state_machine_with_otherwise_guard_and_no_matching_guard
    {
        const int SourceState = 1;
        const int DestinationState = 2;
        const int ErrorState = 3;

        const int Event = 2;

        static PassiveStateMachine<int, int> machine;
        static CurrentStateExtension currentStateExtension;

        private Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();

            currentStateExtension = new CurrentStateExtension();
            machine.AddExtension(currentStateExtension);

            machine.In(SourceState)
                .On(Event)
                    .If(() => false).Goto(ErrorState)
                    .Otherwise().Goto(DestinationState);

            machine.Initialize(SourceState);
            machine.Start();
        };

        private Because of = () => 
            machine.Fire(Event);

        private It should_take_transition_guarded_with_otherwise = () => 
            currentStateExtension.CurrentState.Should().Be(DestinationState);
    }

    [Subject(Concern.Transition)]
    public class When_firing_an_event_onto_a_started_state_machine_with_no_matching_guard
    {
        const int SourceState = 1;
        const int ErrorState = 3;

        const int Event = 2;

        static PassiveStateMachine<int, int> machine;
        static CurrentStateExtension currentStateExtension;
        static bool declined;

        private Establish context = () => 
        {
            machine = new PassiveStateMachine<int, int>();

            currentStateExtension = new CurrentStateExtension();
            machine.AddExtension(currentStateExtension);

            machine.In(SourceState)
                .On(Event)
                    .If(() => false).Goto(ErrorState);

            machine.TransitionDeclined += (sender, e) => declined = true;

            machine.Initialize(SourceState);
            machine.Start();
        };

        private Because of = () =>
            machine.Fire(Event);

        private It should_notify_about_declined_transition = () =>
            declined.Should().BeTrue("TransitionDeclined event should be fired");
    }
}