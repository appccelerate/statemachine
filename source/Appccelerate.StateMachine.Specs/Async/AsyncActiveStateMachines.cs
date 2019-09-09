//-------------------------------------------------------------------------------
// <copyright file="AsyncActiveStateMachines.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Async
{
    using System.Threading;
    using Appccelerate.StateMachine.AsyncMachine;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;

    public class AsyncActiveStateMachines
    {
        [Scenario]
        public void DefaultStateMachineName(
            AsyncActiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            "establish an instantiated active state machine".x(()
                => machine = new AsyncActiveStateMachine<string, int>());

            "establish a state machine reporter".x(()
                => reporter = new StateMachineNameReporter());

            "when the state machine report is generated".x(()
                => machine.Report(reporter));

            "it should use the type of the state machine as name for state machine".x(()
                => reporter.StateMachineName
                    .Should().Be("Appccelerate.StateMachine.AsyncActiveStateMachine<System.String,System.Int32>"));
        }

        [Scenario]
        public void CustomStateMachineName(
            AsyncActiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            const string name = "custom name";

            "establish an instantiated active state machine with custom name".x(()
                => machine = new AsyncActiveStateMachine<string, int>(name));

            "establish a state machine reporter".x(()
                => reporter = new StateMachineNameReporter());

            "when the state machine report is generated".x(()
                => machine.Report(reporter));

            "it should use custom name for state machine".x(()
                => reporter.StateMachineName
                    .Should().Be(name));
        }

        [Scenario]
        public void CustomFactory(
            StandardFactory<string, int> factory)
        {
            "establish a custom factory".x(()
                => factory = A.Fake<StandardFactory<string, int>>());

            "when creating an active state machine".x(() =>
            {
                var machine = new AsyncActiveStateMachine<string, int>("_", factory);

                machine.In("initial").On(42).Goto("answer");
            });

            "it should use custom factory to create internal instances".x(()
                => A.CallTo(factory).MustHaveHappened());
        }

        [Scenario]
        public void EventsQueueing(
            IAsyncStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int firstEvent = 0;
            const int secondEvent = 1;

            "establish an active state machine with transitions".x(() =>
            {
                signal = new AutoResetEvent(false);

                machine = new AsyncActiveStateMachine<string, int>();

                machine.In("A").On(firstEvent).Goto("B");
                machine.In("B").On(secondEvent).Goto("C");
                machine.In("C").ExecuteOnEntry(() => signal.Set());

                machine.Initialize("A");
            });

            "when firing an event onto the state machine".x(async () =>
            {
                await machine.Fire(firstEvent);
                await machine.Fire(secondEvent);
                await machine.Start();
            });

            "it should queue event at the end".x(() =>
                signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state"));
        }

        [Scenario]
        public void PriorityEventsQueueing(
            IAsyncStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int firstEvent = 0;
            const int secondEvent = 1;

            "establish an active state machine with transitions".x(() =>
            {
                signal = new AutoResetEvent(false);

                machine = new AsyncActiveStateMachine<string, int>();

                machine.In("A").On(secondEvent).Goto("B");
                machine.In("B").On(firstEvent).Goto("C");
                machine.In("C").ExecuteOnEntry(() => signal.Set());

                machine.Initialize("A");
            });

            "when firing a priority event onto the state machine".x(async () =>
            {
                await machine.Fire(firstEvent);
                await machine.FirePriority(secondEvent);
                await machine.Start();
            });

            "it should queue event at the front".x(() =>
                signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state"));
        }

        [Scenario]
        public void PriorityEventsWhileExecutingNormalEvents(
            IAsyncStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int firstEvent = 0;
            const int secondEvent = 1;
            const int priorityEvent = 2;

            "establish an active state machine with transitions".x(() =>
            {
                signal = new AutoResetEvent(false);

                machine = new AsyncActiveStateMachine<string, int>();

                machine.In("A").On(firstEvent).Goto("B").Execute(() => machine.FirePriority(priorityEvent));
                machine.In("B").On(priorityEvent).Goto("C");
                machine.In("C").On(secondEvent).Goto("D");
                machine.In("D").ExecuteOnEntry(() => signal.Set());

                machine.Initialize("A");
            });

            "when firing a priority event onto the state machine".x(async () =>
            {
                await machine.Fire(firstEvent);
                await machine.Fire(secondEvent);
                await machine.Start();
            });

            "it should queue event at the front".x(()
                => signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state"));
        }
    }
}