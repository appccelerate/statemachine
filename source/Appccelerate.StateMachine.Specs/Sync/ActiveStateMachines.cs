//-------------------------------------------------------------------------------
// <copyright file="ActiveStateMachines.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Sync
{
    using System.Threading;
    using FakeItEasy;
    using FluentAssertions;
    using Machine;
    using Machine.Events;
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
        public void CustomFactory(
            ActiveStateMachine<string, int> machine,
            IFactory<string, int> factory)
        {
            "establish a custom factory".x(() =>
            {
                factory = A.Fake<IFactory<string, int>>();
            });

            "when creating an active state machine".x(() =>
                machine = new StateMachineDefinitionBuilder<string, int>()
                    .WithCustomFactory(factory)
                    .WithConfiguration(x =>
                        x.In("Initial")
                            .On(42)
                            .Goto("answer")
                            .Execute(() => { }))
                    .Build()
                    .CreateActiveStateMachine());

            "it should use custom factory to create internal instances".x(() =>
                A.CallTo(factory).MustHaveHappened());
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

                machine = new StateMachineDefinitionBuilder<string, int>()
                    .WithConfiguration(x =>
                        x.In("A").On(FirstEvent).Goto("B"))
                    .WithConfiguration(x =>
                        x.In("B").On(SecondEvent).Goto("C"))
                    .WithConfiguration(x =>
                        x.In("C").ExecuteOnEntry(() => signal.Set()))
                    .Build()
                    .CreateActiveStateMachine();

                machine.Initialize("A");
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

                machine = new StateMachineDefinitionBuilder<string, int>()
                    .WithConfiguration(x =>
                        x.In("A").On(SecondEvent).Goto("B"))
                    .WithConfiguration(x =>
                        x.In("B").On(FirstEvent).Goto("C"))
                    .WithConfiguration(x =>
                        x.In("C").ExecuteOnEntry(() => signal.Set()))
                    .Build()
                    .CreateActiveStateMachine();

                machine.Initialize("A");
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