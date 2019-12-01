using System;
using Appccelerate.StateMachine.Machine;

namespace Appccelerate.StateMachine.Samples
{
    public class SimpleStateMachine
    {
        private enum States
        {
            On,
            Off
        }

        private enum Events
        {
            TurnOn,
            TurnOff
        }

        private readonly PassiveStateMachine<States, Events> machine;

        public SimpleStateMachine()
        {
            var builder = new StateMachineDefinitionBuilder<States, Events>();

            builder
                .In(States.Off)
                .On(Events.TurnOn)
                .Goto(States.On)
                .Execute(SayHello);

            builder
                .In(States.On)
                .On(Events.TurnOff)
                .Goto(States.Off)
                .Execute(SayBye);

            builder
                .WithInitialState(States.Off);

            machine = builder
                .Build()
                .CreatePassiveStateMachine();

            machine.Start();
        }

        public void TurnOn()
        {
            machine
                .Fire(
                    Events.TurnOn);
        }

        public void TurnOff()
        {
            machine
                .Fire(
                    Events.TurnOff);
        }

        private void SayHello()
        {
            Console.WriteLine("hello");
        }

        private void SayBye()
        {
            Console.WriteLine("bye");
        }
    }
}