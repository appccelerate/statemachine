//-------------------------------------------------------------------------------
// <copyright file="ActiveStateMachines.cs" company="Appccelerate">
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
    using System.Threading;
    using FluentAssertions;
    using Machine;
    using Xbehave;

    public class ActiveStateMachines
    {
        [Scenario]
        public void DefaultStateMachineName(
            ActiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            "establish an instantiated active state machine".x(() =>
                machine = new StateMachineDefinitionBuilder<string, int>()
                    .WithInitialState("initial")
                    .Build()
                    .CreateActiveStateMachine());

            "establish a state machine reporter".x(() =>
            {
                reporter = new StateMachineNameReporter();
            });

            "when the state machine report is generated".x(() =>
                machine.Report(reporter));

            "it should use the type of the state machine as name for state machine".x(() =>
                reporter.StateMachineName
                    .Should().Be("Appccelerate.StateMachine.ActiveStateMachine<System.String,System.Int32>"));
        }

        [Scenario]
        public void CustomStateMachineName(
            ActiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            const string Name = "custom name";

            "establish an instantiated active state machine with custom name".x(() =>
                machine = new StateMachineDefinitionBuilder<string, int>()
                    .WithInitialState("initial")
                    .Build()
                    .CreateActiveStateMachine(Name));

            "establish a state machine reporter".x(() =>
            {
                reporter = new StateMachineNameReporter();
            });

            "when the state machine report is generated".x(() =>
                machine.Report(reporter));

            "it should use custom name for state machine".x(() =>
                reporter.StateMachineName
                    .Should().Be(Name));
        }

        [Scenario]
        public void EventsQueueing(
            IStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int FirstEvent = 0;
            const int SecondEvent = 1;

            "establish an active state machine with transitions".x(() =>
            {
                signal = new AutoResetEvent(false);

                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder.In("A").On(FirstEvent).Goto("B");
                stateMachineDefinitionBuilder.In("B").On(SecondEvent).Goto("C");
                stateMachineDefinitionBuilder.In("C").ExecuteOnEntry(() => signal.Set());
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("A")
                    .Build()
                    .CreateActiveStateMachine();
            });

            "when firing an event onto the state machine".x(() =>
            {
                machine.Fire(FirstEvent);
                machine.Fire(SecondEvent);
                machine.Start();
            });

            "it should queue event at the end".x(() =>
                signal
                    .WaitOne(1000)
                    .Should()
                    .BeTrue("state machine should arrive at destination state"));
        }

        [Scenario]
        public void PriorityEventsQueueing(
            IStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int FirstEvent = 0;
            const int SecondEvent = 1;

            "establish an active state machine with transitions".x(() =>
            {
                signal = new AutoResetEvent(false);

                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();
                stateMachineDefinitionBuilder.In("A").On(SecondEvent).Goto("B");
                stateMachineDefinitionBuilder.In("B").On(FirstEvent).Goto("C");
                stateMachineDefinitionBuilder.In("C").ExecuteOnEntry(() => signal.Set());
                machine = stateMachineDefinitionBuilder
                    .WithInitialState("A")
                    .Build()
                    .CreateActiveStateMachine();
            });

            "when firing a priority event onto the state machine".x(() =>
            {
                machine.Fire(FirstEvent);
                machine.FirePriority(SecondEvent);
                machine.Start();
            });

            "it should queue event at the front".x(() =>
                signal
                    .WaitOne(1000)
                    .Should()
                    .BeTrue("state machine should arrive at destination state"));
        }
    }
}