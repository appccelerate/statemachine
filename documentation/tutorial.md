# Tutorial
The examples use the enums `States` and `Events` that define the available states and events.

## Define the State Machine
We use the `StateMachineBuilder` to define our state machine. You can find it in the namespace `Appccelerate.StateMachine`. Either use `ForAsyncMachine<TState, TEvent>` or `ForMachine<TState, TEvent>()`  for state machines with support for async/await or without.

```c#
var builder = StateMachineBuilder.ForMachine<int, int>(); // we use a state machine without async support in the tutorial
```

### Define Transitions

#### A simple transition

```c#
builder.In(States.A)
   .On(Events.B).Goto(States.B);
```

If the state machine is in state `A` and receives event `B` then it performs a transition to state `B`.

#### Transition with action

```c#
builder.In(States.A)
    .On(Events.B)
        .Goto(States.B)
        .Execute(() => { /* do something here */ });
```

Actions are defined with `Execute`. Multiple actions can be defined on a single transition. The actions are performed in the same order as they are defined. An action is defined as a delegate. Therefore, you can use lambda expressions or normal delegates.

The following signatures are supported:

```c#
Execute<T>(Action<T> action) // to get access to argument passed to Fire
Execute(Action) // for actions that do not need access to the argument passed to Fire
Execute<T>(Func<T, Task> action) // async machines only, to get access to argument passed to Fire
Execute(Func<T> action) // async machines only, for actions that do not need access to the argument passed to Fire
```

Actions can use the event argument that were passed to the `Fire` method of the state machine.
If the type you specify as the generic parameter type does not match the argument passed with the event, an exception is thrown.

#### Transition with guard

```c#
builder.In(States.A)
    .On(Events.B)
        .If(arguments => false).Goto(States.B1)
        .If(arguments => true).Goto(States.B2);
```

Guards are used to decide which transition to take if there are multiple transitions defined for a single event on a state. Guards can use the event argument that was passed to the `Fire` method of the state machine.
The first transition with a guard that returns `true` is taken.

The signature of guards is as follows:

```c#
If<T>(Func<T, bool> guard) // to get access to the argument passed in Fire
If<T>(Func<T, Task<bool>> guard) // async machines only, to get access to the argument passed in Fire
If(Func<bool> guard) // for guards that do not need access to the parameters passed to Fire
If(Func<Task<bool>> guard) // async machines only, for guards that do not need access to the parameters passed to Fire
```

#### Entry and Exit Actions

```c#
builder.In(States.A)
    .ExecuteOnEntry(() => { /* execute entry action stuff */ }
    .ExecuteOnExit(() => { /* execute exit action stuff */ };
```

When a transition is executed, the exit action of the current state is executed first. Then the transition action is executed. Finally, the entry action of the new current state is executed.

The signatures of entry and exit actions are as follows:

```c#
ExecuteOnEntry(Action action)
ExecuteOnEntry<T>(Action<T> action) // to get access to the argument passed in Fire
ExecuteOnEntry(Func<Task> action) // async machines only
ExecuteOnEntry<T>(Func<T, Task> action) // async machines only, to get access to the argument passed in Fire
```
#### Internal and Self Transitions
Internal and Self transitions are transitions from a state to itself.

When an internal transition is performed then the state is not exited, i.e. no exit or entry action is performed.
When an self transition is performed then the state is exited and re-entered, i.e. exit and entry actions, if any, are performed.

```c#
builder.In(States.A)
    .On(Events.Self).Goto(States.A) // self transition
    .On(Events.Internal)            // internal transition
```

### Define Hierarchies
The following sample defines that `B1`, `B2` and `B3` are sub states of state `B`.
`B1` is defined to be the initial sub state of state `B`.

```c#
builder.DefineHierarchyOn(States.B)
    .WithHistoryType(HistoryType.None)
	.WithInitialSubState(States.B1)
	.WithSubState(States.B2)
	.WithSubState(States.B3);
```

#### History Types
When defining hierarchies then you can define which history type is used when a state is re-entered:
- None: The state enters into its initial sub state. The sub state itself enters its initial sub state and so on until the innermost nested state is reached.
- Deep: The state enters into its last active sub state. The sub state itself enters into its last active state and so on until the innermost nested state is reached.
- Shallow: The state enters into its last active sub state. The sub state itself enters its initial sub state and so on until the innermost nested state is reached.

### Initialize the State Machine
The state machine need to know in which state it should start.

```c#
builder.WithInitialState(States.A)
```

## Build the Definition
Then we build our definition:

```c#
var definition = builder.Build()
```

The definition can then be used to spawn state machines - as many as you need.
For example, you can store the definition as a singleton in your ASP.NET backend and create a state machine for every new request.

## Create, Start and Stop State Machine
Once you have defined your state machine then you can start using it.
Create either a passive or an active state machine, with support for async/await or without:

```c#
var machine = definition.CreatePassiveStateMachine() // create a passive state machine
```

```c#
var machine = definition.CreateActiveStateMachine() // create an active state machine
```

Once you start the state machine, it will execute events fired on it:

```c#
machine.Start();
```
Events are processed only if the state machine is started. However, you can queue up events before starting the state machine. As soon as you start the state machine, it will start performing the events.

To suspend event processing, you can stop the state machine:

```c#
machine.Stop();
```

If you want, you can then start the state machine again, then stop it, start again and so on.

## Fire Events
To get the state machine to do its work, you send events to it:

```c#
machine.Fire(Events.B);
```

This fires the event `B` onto the state machine and it will perform the corresponding transition for this event on its current state.

You can also pass an argument to the state machine that can be used by transition actions and guards:

```c#
machine.Fire(Events.B, anArgument);
```

Another possibility is to send a priority event:

```c#
machine.FirePriority(Events.B);
```

In this case, the event `B` is enqueued in front of all queued events. This is especially helpful in error scenarios to go to the error state immediately without performing any other already queued events first.

That's it for the tutorial. See the rest of the documentation for more details on specific topics.

Happy coding!