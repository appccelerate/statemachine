# Async Active State Machine
The `AsyncActiveStateMachine` is a state machine that that supports `async/await` for transition actions, entry/exit actions, guards and save/load and that has its own worker thread on which the transitions are performed. The program flow returns to the caller of the state machine immediately after the event was queued by a call to `Fire`.

## State Machine Events Execution
The state machine executes the events on its worker thread.

## Actions Queueing Events
When an action fires an event onto the state machine then this event is queued and the action continues. Note that other threads may add events themselves. There is no guarantee that the event fired by the action is the next event actually executed.

## Thread-safety
It is save to call an async active state machine from multiple threads.