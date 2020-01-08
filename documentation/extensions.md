# Extensions
Extensions are used to change or access internals of the state machine. For example logging can easily be implemented with an extension.

## Changing the Behaviour of a State Machine
Extensions can change the behaviour of the state machine in these ways:

- change the state into which the state machine is initialized
- change the event and/or event arguments fired on the state machine
- change the exceptions thrown by the state machine with custom exceptions
- firing events on the state machine as a reaction to a call on the extension (e.g. perform transition to the error state on an exception)

## Interface
A state machine extension has to implement the `IExtension<TState, TEvent>` interface - either from the namespace `Appccelerate.State;achine.Async.Extensions` for state machines with async/await support, or `Appccelerate.StateMachine.Sync.Extensions` for state machines without async/await support.

You can use either the `AsyncExtensionBase` or `ExtensionBase` from the same namespaces to derive your extensions. The extension base class implements all interface members with virtual empty methods.