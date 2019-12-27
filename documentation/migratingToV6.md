# Migrating to Version 6

Version 6 supports nullable reference types.

Supporting nullable reference types introduced a lot of internal changes. I took the opportunity to clean the code base as a whole. Unfortunately, that introduced a lot of changes in namespaces and APIs. Most changes, however, are for detail stuff that you probably never have used directly anyway.

## New entry point for defining a state machine
You can now use `StateMachineBuilder` as an entry point to define any state machine, with or without async/await and passive or active.

Example:

```c#
using Appccelerate.StateMachine.Machine;


var builder = StateMachineBuilder.ForMachine<States, Events>();

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
```