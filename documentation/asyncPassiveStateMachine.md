# Async Passive State Machine
The `AsyncPassiveStateMachine` is a state machine that supports `async/await` for transition actions, entry/exit actions, guards and save/load. Like the `PassiveStateMachine` it performs transitions on the same thread as the code calling the state machine. That means that when the call to `Fire` returns to the caller and the returned task is awaited then the transition is performed.

## State Machine Events Execution
All events fired onto the state machine are executed on the same `async/await` chain as the caller.

## Actions queueing events
When a transition, exit or entry action fires an event onto the state machine then this event is executed immediately after the initial event is processed and before task returned by `Fire` is completed.

## Thread-safety
The async passive state machine is not thread-safe. It should be called from only a single thread during its lifetime.