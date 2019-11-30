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
