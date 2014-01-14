//-------------------------------------------------------------------------------
// <copyright file="StartStopSpecification.cs" company="Appccelerate">
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

    [Subject(Concern.StartStop)]
    public class When_starting_an_initialized_state_machine : InitializedTwoStateStateMachineSpecification
    {
        Because of = () =>
            {
                machine.Fire(Event);
                machine.Fire(Event);
                machine.Fire(Event);

                machine.Start();
            };

        It should_execute_queued_events = () =>
            {
                extension.RecordedFiredEvents.Should().HaveCount(3);
            };
    }

    [Subject(Concern.StartStop)]
    public class When_stopping_a_running_state_machine : InitializedTwoStateStateMachineSpecification
    {
        Because of = () =>
        {
            machine.Start();
            machine.Stop();

            machine.Fire(Event);
        };

        It should_queue_events = () =>
            {
                extension.RecordedQueuedEvents.Should().HaveCount(1);
            };
    }

    [Subject(Concern.StartStop)]
    public class InitializedTwoStateStateMachineSpecification
    {
        protected const int A = 0;
        protected const int B = 1;
        protected const int Event = 0;

        protected static PassiveStateMachine<int, int> machine;

        protected static RecordEventsExtension extension;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<int, int>();

            extension = new RecordEventsExtension();
            machine.AddExtension(extension);

            machine.In(A)
                .On(Event).Goto(B);

            machine.In(B)
                .On(Event).Goto(A);

            machine.Initialize(A);
        };
    }
}