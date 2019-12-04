# Migrating to Version 5

In Version 5, we split the state machine usage into three distinct steps:
- Define the state machine
- Create a state machine from the definition
- Run the state machine

State machines can now be quickly created from an existing definition.

An example state machine prior to version 5:
```c#
var machine = new PassiveStateMachine<States, Events>("my state machine");

machine
    .In(States.Off)
    .On(Events.TurnOn)
        .Goto(States.On)
        .Execute(SayHello);

machine
    .In(States.On)
    .On(Events.TurnOff)
        .Goto(States.Off)
        .Execute(SayBye);

machine
    .Initialize(States.Off);

machine
    .Start();
```

In version 5, the same state machine looks like this:
```c#
// create a definition builder
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
    .WithInitialState(States.Off); // the initial state is now set during definition time

// create the definition to later spawn state machines from it
var definition = builder
    .Build();

// create as many state machines as you need from the same definition
var machine =    
    .CreatePassiveStateMachine("my state machine"); // you can also create an active state machine

machine
    .Start();
```

Make sure you use the correct `StateMachineDefinitionBuilder`:
- from namespace `Appccelerate.StateMachine.Machine` for state machines **without** async/await support
- from namespace `Appccelerate.StateMachine.AsyncMachine` for state machines **with** async/await support

## Changes in extension API

The extension API is changed in version 5, too.
Instead of mutable states, the state definition is passed to the extensions.