namespace Appccelerate.StateMachine.Samples
{
    using Appccelerate.StateMachine.Machine;

    public class Elevator
    {
        private readonly PassiveStateMachine<States, Events> elevator;

        private enum States
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

        private enum Events
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
            var builder = new StateMachineDefinitionBuilder<States, Events>();

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
    }
}