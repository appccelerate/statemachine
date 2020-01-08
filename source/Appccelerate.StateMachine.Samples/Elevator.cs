namespace Appccelerate.StateMachine.Samples
{
    using System;
    using System.IO;
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Machine.Reports;
    using Xunit;
    using Xunit.Abstractions;

    public class Elevator
    {
        private readonly PassiveStateMachine<States, Events> elevator;

        internal enum States
        {
            Healthy,
            OnFloor,
            Moving,
            MovingUp,
            MovingDown,
            DoorOpen,
            DoorClosed,
            Error
        }

        internal enum Events
        {
            GoUp,
            GoDown,
            OpenDoor,
            CloseDoor,
            Stop,
            Error,
            Reset
        }

        public Elevator()
        {
            var builder = StateMachineBuilder.ForMachine<States, Events>();

            builder.DefineHierarchyOn(States.Healthy)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(States.OnFloor)
                .WithSubState(States.Moving);

            builder.DefineHierarchyOn(States.Moving)
                .WithHistoryType(HistoryType.Shallow)
                .WithInitialSubState(States.MovingUp)
                .WithSubState(States.MovingDown);

            builder.DefineHierarchyOn(States.OnFloor)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.DoorClosed)
                .WithSubState(States.DoorOpen);

            builder.In(States.Healthy)
                .On(Events.Error).Goto(States.Error);

            builder.In(States.Error)
                .On(Events.Reset).Goto(States.Healthy)
                .On(Events.Error);

            builder.In(States.OnFloor)
                .ExecuteOnEntry(this.AnnounceFloor)
                .ExecuteOnExit(Beep)
                .ExecuteOnExit(Beep) // just beep a second time
                .On(Events.CloseDoor).Goto(States.DoorClosed)
                .On(Events.OpenDoor).Goto(States.DoorOpen)
                .On(Events.GoUp)
                    .If(CheckOverload).Goto(States.MovingUp)
                    .Otherwise().Execute(this.AnnounceOverload)
                .On(Events.GoDown)
                    .If(CheckOverload).Goto(States.MovingDown)
                    .Otherwise().Execute(this.AnnounceOverload);
            builder.In(States.Moving)
                .On(Events.Stop).Goto(States.OnFloor);

            builder.WithInitialState(States.OnFloor);

            var definition = builder
                .Build();

            elevator = definition
                .CreatePassiveStateMachine("Elevator");

            elevator.Start();
        }

        public void GoToUpperLevel()
        {
            this.elevator.Fire(Events.CloseDoor);
            this.elevator.Fire(Events.GoUp);
            this.elevator.Fire(Events.OpenDoor);
        }

        public void GoToLowerLevel()
        {
            this.elevator.Fire(Events.CloseDoor);
            this.elevator.Fire(Events.GoDown);
            this.elevator.Fire(Events.OpenDoor);
        }

        public void Error()
        {
            this.elevator.Fire(Events.Error);
        }

        public void Stop()
        {
            this.elevator.Fire(Events.Stop);
        }

        public void Reset()
        {
            this.elevator.Fire(Events.Reset);
        }

        private void AnnounceFloor()
        {
            /* announce floor number */
        }

        private void AnnounceOverload()
        {
            /* announce overload */
        }

        private void Beep()
        {
            /* beep */
        }

        private bool CheckOverload()
        {
            return false;
        }

        internal void Report(
            IStateMachineReport<States, Events> reporter)
        {
            this.elevator
                .Report(
                    reporter);
        }
    }

    public class Reports
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Reports(
            ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Yed()
        {
            var elevator = new Elevator();

            var writer = new StringWriter();
            var reporter = new YEdStateMachineReportGenerator<Elevator.States, Elevator.Events>(
                writer);

            elevator.Report(
                reporter);

            testOutputHelper
                .WriteLine(writer.ToString());
        }

        [Fact]
        public void Csv()
        {
            var elevator = new Elevator();

            var statesWriter = new StringWriter();
            var transitionsWriter = new StringWriter();
            var reporter = new CsvStateMachineReportGenerator<Elevator.States, Elevator.Events>(
                statesWriter,
                transitionsWriter);

            elevator.Report(
                reporter);

            testOutputHelper
                .WriteLine(statesWriter.ToString());
            testOutputHelper
                .WriteLine(transitionsWriter.ToString());
        }

        [Fact]
        public void StateMachineReport()
        {
            var elevator = new Elevator();

            var reporter = new StateMachineReportGenerator<Elevator.States, Elevator.Events>();

            elevator.Report(
                reporter);

            testOutputHelper
                .WriteLine(reporter.Result);
        }
    }
}