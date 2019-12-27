# Passive State Machine
The `PassiveStateMachine` is a state machine that performs transitions on the same thread as the code calling the state machine. That means that when the call to `Fire` returns to the caller then the transition is performed.

## State Machine Events Execution
All events fired onto the state machine are executed on the same thread as the caller.

## Actions queueing events
When a transition, exit or entry action fires an event onto the state machine then this event is executed immediately after the initial event is processed and before the process flow is returned to the caller of the state machine.

## Thread-safety
The passive state machine is not thread-safe. It should be called from only a single thread during its lifetime.