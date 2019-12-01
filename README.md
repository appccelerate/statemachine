I'm working on moving the documention from www.appccelerate.com (where it is not up-to-date :-( ) to here because it is less time consuming to update the documentation.

## Features

- use enums, ints, strings or your own class for states and events - resulting in single class state machines.
- transition, entry and exit actions.
- transition guards
- hierarchical states with history behavior to initialize state always to same state or last active state.
- supports async/await
- fluent definition syntax.
- passive state machine handles state transitions synchronously.
- active state machine handles state transitions asynchronously on the worker thread of the state machine.
- active state machine is thread-safe
- current state, queued events and history states can be persisted
- extension support to extend functionality of state machine.
- extensible thorough logging simplifies debugging.
- state machine reports as text, csv or yEd diagram. Or write your own report.

## StateMachine

Appccelerate contains four different state machines:
- Passive State Machine
- Active State Machine
- Async Passive State Machine
- Async Active State Machine

Both async and not-async variants implement an interface called `IStateMachine`. For better testability and flexibility, I suggest that you reference your state machine only by the interface and use dependency injection to get either of the implementations. You can use then a passive state machine as a replacement for an active one in your tests to simplify the tests because everything is run on the same thread.

### States and Events
A state machine is defined using States and Events. States and events have to be `IComparables` (`enum`, `int`, `string`, ...). If you have a well known set of states and events then I suggest you use enums which make the code much more readable. If you plan to build some flexibility into your state machine (e.g. add states, transitions in base classes) then you better use an "open" type like `string` or `integer`.

### Transitions
Transitions are state switches that are executed in response to an event that was fired onto the state machine. You can define per state and event which transition is taken and therefore which state to go to.

### Actions
You can define actions either on transitions or on entry or exit of a state. Transition actions are executed when the transition is taken as the response to an event. The entry and exit actions of a state are excuted when the state machine enters or exits the state due to a taken transition. In case of hierarchical states, multiple entry and exit actions can be executed.

### Guards
Guards give you the possibility to decide which transition is executed depending on a boolean criteria. When an event is fired onto the state machine, it takes all transitions defined in the current state for the fired event and executes the first transition with a guard returning true.

### Extensions
Extensions can be used to extend the functionality of the state machine. They provide for example a simple way to write loggers.

### Reports
Out of the box, Appccelerate provides a textual report, a csv report and a yEd diagram reporter. You can add your own reports by just implementing `IStateMachineReport`.

## Simple Sample State Machine

```c#
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
```

## More Documentation

- [Tutorial](documentation/tutorial.md)