# Exception Handling
The state machine catches all exceptions that are thrown. This is to prevent the caller from having to handle such an exception. The reason is that the caller normally does not know enough about the state machine to take a clever action to handle an exception.

Therefore, the state machine fires events in case of an exception. The "holder" of the state machine can register to these events and act accordingly:

```c#
stateMachine.TransitionExceptionThrown += (sender, eventArgs) =>
    {
        // react to error
    };
```

Normally, you want send the state machine in an error state to reflect this situation.

If you do not register the error event, the state machine will throw exceptions anyway to prevent that exceptions are swallowed. **This is especially useful in unit tests**. Otherwise you could get green tests although some code throws exceptions.

You should try to prevent exceptions from being thrown at the state machine. Handling exceptions meaningfully over the state machine is most times very difficult.