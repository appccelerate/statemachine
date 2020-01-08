# Events
The state machine provides the following events for you to listen to:
- `TransitionDeclined`
- `TransitionExceptionThrown`
- `TransitionBegin`
- `TransitionCompleted`

## `TransitionDeclined`
This event is fired whenever there is no transition defined in the current state or one of its parent states for the fired state machine event. The state machine will remain in the current state.

This event is especially useful in tests to make sure that no events are skipped.

## `TransitionExceptionThrown`
This event is fired whenever there occurs an exception during execution of a transition, i.e. in a guard, exit, transition or entry action.

This event is useful because the code calling the `Fire` method of the state machine can normally not handle a transition exception meaningfully. If you want to log these exceptions, you can register this event and log the exception in the handler method.

If this event is not registered, the state machine will rethrow the exception so that no exception is silently swallowed. Use this in unit tests to make sure that no exceptions are swallowed.

## `TransitionBegin`
This event is fired whenever a transition is to be executed.

## `TransitionCompleted`
This event is fired whenever a transition succeeded.

Although these events can be used to implement simple logging or handling exceptions of the state machine, [extensions](extensions.md) are better suited for logging and exception handling.