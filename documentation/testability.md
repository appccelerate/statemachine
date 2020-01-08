# Testability
To test code that uses a state machine, we first replace active state machines with passive ones. This can easily be achieved by injecting an `IStateMachine`/`IAsyncStateMachine` into the class under test. In production code an active state machine is injected, in test code, a passive state machine is used. 

Passive and active state machine have exactly the same behavior but for one feature. Active state machines have their own worker thread. But tests are much simpler if they are pure synchronous, therefore this it is a good thing for us during testing when the state machine runs synchronously.

In production code, you should add handlers for the exception event of the state machine (`TransitionExceptionThrown`). In tests, you can use the fact that the state machine will re-throw any exception if there is no event handler registered. This makes sure that no exceptions are swallowed in your tests.