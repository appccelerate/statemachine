# Actions
Actions are used to execute code when a transition is executed. Actions can be defined on a transition or on a state as either entry or exit actions.

## Transition Actions
The actions attached to a transition are executed when the transition is taken. Several actions on a single transition can be defined with multiple `Execute` calls. There exist several overloads of the `Execute` method so you can define actions is different ways:

```c#
stateMachine.In(States.A)
    .On(Events.B).Goto(States.B)
        .Execute(() => { ... })    // use lambda
        .Execute(this.SomeMethod)  // use a method with signature void Method()
```
State machines supporting async/await always provide an overload that takes a `Task` as a return value, too.

### Accessing Event Argument
When you pass an argument in the `Fire` method on the state machine, this argument gets passed to the transition action in case the transition action takes an argument.

```
stateMachine.In(States.A)
    .On(Events.B).Goto(States.B)
        .Execute((int i) => { ... })    // use lambda
        .Execute<int>(this.SomeMethod)  // use a method with signature void Method(int i)
```

You can use argumentless lambdas or methods in case an argument was passed along with `Fire`. However, if the argument passed cannot be casted to the type specified by a transition action with an argument, an exception is thrown. Therefore, I suggest that you use only one type of event argument inside a single state machine and if necessary, cast the value yourself in the action if possible.

When `Fire` is invoked without an event argument but the transition action expects an argument, the default value of the expected argument type is passed to the action.

## Entry and Exit Actions
Entry and exit actions are attached to a state. They get executed when a transition ends or starts in this state. Several entry and exit actions can be defined on a single state.

```c#
stateMachine.In(States.A)
    .ExecuteOnEntry(() => { ... })   // use lambda
    .ExecuteOnEntry(this.SomeMethod) // method with signature void Method()
    .ExecuteOnExit(() => { ... })    // use lambda
    .ExecuteOnExit(this.SomeMethod)  // method with signature void Method()
```

### Parametrized Entry and Exit Actions
Parametrized entry and exit actions simplify using actions that toggle states when the state is entered or left. The same action can be used, with different parameter values. You can for example use this to enable a button when the state is entered and disable the button when the state is left. To do this, define a `ToggleButton` method that takes a `bool` as input and specify `true` as the parameter in the entry action and `false` in the exit action:

```c#
stateMachine.In(States.A)
    .ExecuteOnEntry(this.ToggleButton, true)
    .ExecuteOnExit(this.ToggleButton, false);   

private void ToggleButton(bool enable)
{
    this.someButton.Enabled = enable;
}
```

Of course, you can use any type for a parameter.

### Accessing Event Argument
Entry and exit actions can access the event argument, too.

```c#
stateMachine.In(States.A)
    .ExecuteOnEntry((string s) => { ... })
    .ExecuteOnEntry<string>(this.SomeMethodWithStringArgument)
    .ExecuteOnExit((string s) => { ... })
    .ExecuteOnExit<string>(this.SomeMethodWithStringArgument);
```

Important: When the state machine is initialized and enters its initial state, there won't be an event argument. Therefore, you have to make sure that either the initial state does not have an entry action with an argument or can execute when the default value of the type of the expected argument is pass to it.

When `Fire` is invoked without an event argument but the entry or exit action expects an argument, the default value of the expected argument type is passed to the action.

## Order of Execution
Actions are executed in the following order:

1. Exit actions in order of definition
2. Transition actions in order of definition
3. Entry actions in order of definition

Actions of the same kind are always executed in the order in which you have defined them in the state machine definition.