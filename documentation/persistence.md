# Persistence
The persistence mechanism can be used to save and later reload the current state, queued events and the history states (which state was last active in a super state) of the state machine.

Saving a state machine:

```c#
stateMachine.Save(saver);
```

Loading a state machine:
```c#
stateMachine.Load(loader);
```

## Implementing the saver and loader
The saver needs to implement the interface `IStateMachineSaver<TState, TEvent>`. The state machine will then call your saver and pass to it the current state, queued events and a dictionary containing the history states (key = id of super state, value = id of this super state's last active state). If there is no current state (the state machine is not started yet), no call to save the current state will be made. Also, only super states with a last active state are contained in the dictionary. Super states that were never visited and therefore have no last active state are omitted.

The saver gets the current state in an `Option<TState>`. Only if the state machine was once started, the option will contain the current state; otherwise it will be `Option<TState>.None`.

The loader needs to implement the interface `IStateMachineLoader<TState, TEvent>`. The state machine will then call your loader to get the current state, the queued events and the history states. You should return exactly the same values, which were passed to the saver (the same values, not necessarily the same objects, of course).

Currently, there are no default savers and loaders available. If you write one, please contribute it :-)

# When is it save to call save?
You should only call `Save` on the state machine when the state machine is not executing.
So make sure that either the state machine is stopped, there are no queued events and no code will call `Fire`, save the state machine in an extension when the transition was completed or as a reaction to the `TransitionCompleted` event.