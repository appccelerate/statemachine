//-------------------------------------------------------------------------------
// <copyright file="StartStop.cs" company="Appccelerate">
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

    public class StartStop
    {
        private const int A = 0;
        private const int B = 1;
        private const int Event = 0;

        private PassiveStateMachine<int, int> machine;
        private RecordEventsExtension extension;

        [Background]
        public void Background()
        {
            "establish a state machine".x(() =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
                stateMachineDefinitionBuilder
                    .In(A)
                        .On(Event)
                        .Goto(B);
                stateMachineDefinitionBuilder
                    .In(B)
                        .On(Event)
                        .Goto(A);
                this.machine = stateMachineDefinitionBuilder
                    .WithInitialState(A)
                    .Build()
                    .CreatePassiveStateMachine();

                this.extension = new RecordEventsExtension();
                this.machine.AddExtension(this.extension);
            });
        }

        [Scenario]
        public void Starting()
        {
            "establish some queued events".x(() =>
            {
                this.machine.Fire(Event);
                this.machine.Fire(Event);
                this.machine.Fire(Event);
            });

            "when starting".x(() =>
                this.machine.Start());

            "it should execute queued events".x(() =>
                this.extension.RecordedFiredEvents.Should().HaveCount(3));
        }

        [Scenario]
        public void Stopping()
        {
            "establish started state machine".x(() =>
                this.machine.Start());

            "when stopping a state machine".x(() =>
                this.machine.Stop());

            "when firing events onto the state machine".x(() =>
                 this.machine.Fire(Event));

            "it should queue events".x(() =>
                this.extension.RecordedQueuedEvents.Should().HaveCount(1));
        }
    }
}