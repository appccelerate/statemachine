//-------------------------------------------------------------------------------
// <copyright file="AsyncPassiveStateMachines.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Specs.Async
{
    using AsyncMachine;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;

    public class AsyncPassiveStateMachines
    {
        [Scenario]
        public void DefaultStateMachineName(
            AsyncPassiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            "establish an instantiated passive state machine".x(()
                => machine = new AsyncPassiveStateMachine<string, int>());

            "establish a state machine reporter".x(()
                => reporter = new StateMachineNameReporter());

            "when the state machine report is generated".x(()
                => machine.Report(reporter));

            "it should use the type of the state machine as name for state machine".x(()
                => reporter.StateMachineName
                    .Should().Be("Appccelerate.StateMachine.AsyncPassiveStateMachine<System.String,System.Int32>"));
        }

        [Scenario]
        public void CustomStateMachineName(
            AsyncPassiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            const string name = "custom name";

            "establish an instantiated passive state machine with custom name".x(()
                => machine = new AsyncPassiveStateMachine<string, int>(name));

            "establish a state machine reporter".x(()
                => reporter = new StateMachineNameReporter());

            "when the state machine report is generated".x(()
                => machine.Report(reporter));

            "it should use custom name for the state machine".x(()
                => reporter.StateMachineName
                    .Should().Be(name));
        }

        [Scenario]
        public void CustomFactory(
            StandardFactory<string, int> factory)
        {
            "establish a custom factory".x(()
                => factory = A.Fake<StandardFactory<string, int>>());

            "when creating a passive state machine".x(() =>
            {
                var machine = new AsyncPassiveStateMachine<string, int>("_", factory);

                machine.In("initial").On(42).Goto("answer");
            });

            "it should use custom factory to create internal instances".x(()
                => A.CallTo(factory).MustHaveHappened());
        }

        [Scenario]
        public void EventsQueueing(
            IAsyncStateMachine<string, int> machine)
        {
            const int firstEvent = 0;
            const int secondEvent = 1;

            bool arrived = false;

            "establish a passive state machine with transitions".x(() =>
            {
                machine = new AsyncPassiveStateMachine<string, int>();

                machine.In("A").On(firstEvent).Goto("B");
                machine.In("B").On(secondEvent).Goto("C");
                machine.In("C").ExecuteOnEntry(() => arrived = true);

                machine.Initialize("A");
            });

            "when firing an event onto the state machine".x(() =>
            {
                machine.Fire(firstEvent);
                machine.Fire(secondEvent);
                machine.Start();
            });

            "it should queue event at the end".x(()
                => arrived.Should().BeTrue("state machine should arrive at destination state"));
        }

        [Scenario]
        public void PriorityEventsQueueing(
            IAsyncStateMachine<string, int> machine)
        {
            const int firstEvent = 0;
            const int secondEvent = 1;

            bool arrived = false;

            "establish a passive state machine with transitions".x(() =>
            {
                machine = new AsyncPassiveStateMachine<string, int>();

                machine.In("A").On(secondEvent).Goto("B");
                machine.In("B").On(firstEvent).Goto("C");
                machine.In("C").ExecuteOnEntry(() => arrived = true);

                machine.Initialize("A");
            });

            "when firing a priority event onto the state machine".x(() =>
            {
                machine.Fire(firstEvent);
                machine.FirePriority(secondEvent);
                machine.Start();
            });

            "it should queue event at the front".x(()
                => arrived.Should().BeTrue("state machine should arrive at destination state"));
        }
    }
}