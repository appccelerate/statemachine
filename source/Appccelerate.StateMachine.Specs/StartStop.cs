//-------------------------------------------------------------------------------
// <copyright file="StartStop.cs" company="Appccelerate">
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
            "establish initialized state machine"._(() =>
            {
                this.machine = new PassiveStateMachine<int, int>();

                this.extension = new RecordEventsExtension();
                this.machine.AddExtension(extension);

                this.machine.In(A)
                    .On(Event).Goto(B);

                this.machine.In(B)
                    .On(Event).Goto(A);

                this.machine.Initialize(A);
            });
        }

        [Scenario]
        public void Starting()
        {
            "establish some queued events"._(() =>
                {
                    this.machine.Fire(Event);
                    this.machine.Fire(Event);
                    this.machine.Fire(Event);
                });

            "when starting"._(() =>
                this.machine.Start());

            "it should execute queued events"._(() =>
                this.extension.RecordedFiredEvents.Should().HaveCount(3));
        }

        [Scenario]
        public void Stopping()
        {
            "establish started state machine"._(() =>
                this.machine.Start());

            "when stopping a state machine"._(() => 
                this.machine.Stop());

            "when firing events onto the state machine"._(() =>
                 this.machine.Fire(Event));

            "it should queue events"._(() => 
                this.extension.RecordedQueuedEvents.Should().HaveCount(1));
        }
    }
}