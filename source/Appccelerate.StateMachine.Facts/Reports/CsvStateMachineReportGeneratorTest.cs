//-------------------------------------------------------------------------------
// <copyright file="CsvStateMachineReportGeneratorTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Reports
{
    using System.IO;

    using FluentAssertions;

    using Xunit;

    public class CsvStateMachineReportGeneratorTest
    {
        private readonly MemoryStream stateStream;

        private readonly Stream transitionsStream;

        private readonly CsvStateMachineReportGenerator<States, Events> testee;

        public CsvStateMachineReportGeneratorTest()
        {
            this.stateStream = new MemoryStream();
            this.transitionsStream = new MemoryStream();

            this.testee = new CsvStateMachineReportGenerator<States, Events>(this.stateStream, this.transitionsStream);
        }

        ~CsvStateMachineReportGeneratorTest()
        {
            this.stateStream.Dispose();
        }

        /// <summary>
        /// Some test states for simulating an elevator.
        /// </summary>
        private enum States
        {
            /// <summary>Elevator has an Error</summary>
            Error,

            /// <summary>Elevator is healthy, i.e. no error</summary>
            Healthy,

            /// <summary>The elevator is moving (either up or down)</summary>
            Moving,

            /// <summary>The elevator is moving down.</summary>
            MovingUp,

            /// <summary>The elevator is moving down.</summary>
            MovingDown,

            /// <summary>The elevator is standing on a floor.</summary>
            OnFloor,

            /// <summary>The door is closed while standing still.</summary>
            DoorClosed,

            /// <summary>The door is open while standing still.</summary>
            DoorOpen
        }

        /// <summary>
        /// Some test events for the elevator
        /// </summary>
        private enum Events
        {
            /// <summary>An error occurred.</summary>
            ErrorOccurred,

            /// <summary>Reset after error.</summary>
            Reset,

            /// <summary>Open the door.</summary>
            OpenDoor,

            /// <summary>Close the door.</summary>
            CloseDoor,

            /// <summary>Move elevator up.</summary>
            GoUp,

            /// <summary>Move elevator down.</summary>
            GoDown,

            /// <summary>Stop the elevator.</summary>
            Stop
        }

        [Fact]
        public void Report()
        {
            var elevator = new PassiveStateMachine<States, Events>("Elevator");

            elevator.DefineHierarchyOn(States.Healthy)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(States.OnFloor)
                .WithSubState(States.Moving);

            elevator.DefineHierarchyOn(States.Moving)
                .WithHistoryType(HistoryType.Shallow)
                .WithInitialSubState(States.MovingUp)
                .WithSubState(States.MovingDown);

            elevator.DefineHierarchyOn(States.OnFloor)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.DoorClosed)
                .WithSubState(States.DoorOpen);

            elevator.In(States.Healthy)
                .On(Events.ErrorOccurred).Goto(States.Error);

            elevator.In(States.Error)
                .On(Events.Reset).Goto(States.Healthy)
                .On(Events.ErrorOccurred);

            elevator.In(States.OnFloor)
                .ExecuteOnEntry(AnnounceFloor)
                .ExecuteOnExit(Beep)
                .ExecuteOnExit(Beep)
                .On(Events.CloseDoor).Goto(States.DoorClosed)
                .On(Events.OpenDoor).Goto(States.DoorOpen)
                .On(Events.GoUp)
                    .If(CheckOverload).Goto(States.MovingUp)
                    .Otherwise()
                        .Execute(AnnounceOverload)
                        .Execute(Beep)
                .On(Events.GoDown)
                    .If(CheckOverload).Goto(States.MovingDown)
                    .Otherwise().Execute(AnnounceOverload);

            elevator.In(States.Moving)
                .On(Events.Stop).Goto(States.OnFloor);

            elevator.Initialize(States.OnFloor);

            elevator.Report(this.testee);

            string statesReport;
            string transitionsReport;
            this.stateStream.Position = 0;
            using (var reader = new StreamReader(this.stateStream))
            {
                statesReport = reader.ReadToEnd();
            }

            this.transitionsStream.Position = 0;
            using (var reader = new StreamReader(this.transitionsStream))
            {
                transitionsReport = reader.ReadToEnd();
            }

            const string ExpectedTransitionsReport = "Source;Event;Guard;Target;ActionsHealthy;ErrorOccurred;;Error;OnFloor;CloseDoor;;DoorClosed;OnFloor;OpenDoor;;DoorOpen;OnFloor;GoUp;CheckOverload;MovingUp;OnFloor;GoUp;;internal transition;AnnounceOverload, BeepOnFloor;GoDown;CheckOverload;MovingDown;OnFloor;GoDown;;internal transition;AnnounceOverloadMoving;Stop;;OnFloor;Error;Reset;;Healthy;Error;ErrorOccurred;;internal transition;";
            const string ExpectedStatesReport = "Source;Entry;Exit;ChildrenHealthy;;;OnFloor, MovingOnFloor;AnnounceFloor;Beep, Beep;DoorClosed, DoorOpenMoving;;;MovingUp, MovingDownMovingUp;;;MovingDown;;;DoorClosed;;;DoorOpen;;;Error;;;";

            statesReport.Replace("\n", string.Empty).Replace("\r", string.Empty)
                .Should().Be(ExpectedStatesReport.Replace("\n", string.Empty).Replace("\r", string.Empty));

            transitionsReport.Replace("\n", string.Empty).Replace("\r", string.Empty)
                .Should().Be(ExpectedTransitionsReport.Replace("\n", string.Empty).Replace("\r", string.Empty));
        }

        private static void Beep()
        {
        }

        private static bool CheckOverload()
        {
            return true;
        }

        private static void AnnounceFloor()
        {
        }

        private static void AnnounceOverload()
        {
        }
    }
}