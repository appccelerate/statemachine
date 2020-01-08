# Appccelerate StateMachine Specifications
*The specifications were created from the [xBehave.net](https://xbehave.github.io/) tests of the state machine using [xBehaveReportGenerator](https://github.com/ursenzler/xBehaveReportGenerator)*

## Async
### AsyncActiveStateMachines
#### CustomStateMachineName
01. establish an instantiated active state machine with custom name
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use custom name for state machine

#### DefaultStateMachineName
01. establish an instantiated active state machine
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use the type of the state machine as name for state machine

#### EventsQueueing
01. establish an active state machine with transitions
02. when firing an event onto the state machine
03. it should queue event at the end

#### PriorityEventsQueueing
01. establish an active state machine with transitions
02. when firing a priority event onto the state machine
03. it should queue event at the front

#### PriorityEventsWhileExecutingNormalEvents
01. establish an active state machine with transitions
02. when firing a priority event onto the state machine
03. it should queue event at the front

### AsyncPassiveStateMachines
#### CustomStateMachineName
01. establish an instantiated passive state machine with custom name
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use custom name for the state machine

#### DefaultStateMachineName
01. establish an instantiated passive state machine
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use the type of the state machine as name for state machine

#### EventsQueueing
01. establish a passive state machine with transitions
02. when firing an event onto the state machine
03. it should queue event at the end

#### PriorityEventsQueueing
01. establish a passive state machine with transitions
02. when firing a priority event onto the state machine
03. it should queue event at the front

### CustomTypes
#### CustomTypesForStatesAndEvents
01. establish a state machine with custom types for states and events
02. when using the state machine
03. it should use equals to compare states and events

### EntryActions
#### EntryAction
01. establish a state machine with entry action on a state
02. when entering the state
03. it should execute the synchronous entry action
04. it should execute the asynchronous entry action

#### EntryActionWithParameter
01. establish a state machine with entry action with parameter on a state
02. when entering the state
03. it should execute the entry synchronous action
04. it should pass parameter to the synchronous entry action
05. it should execute the asynchronous entry action
06. it should pass parameter to the asynchronous entry action

#### EventArgument
01. establish a state machine with an entry action taking an event argument
02. when entering the state
03. it should pass event argument to synchronous entry action
04. it should pass event argument to asynchronous entry action

#### MultipleEntryActions
01. establish a state machine with several entry actions on a state
02. when entering the state
03. it should execute all entry actions

### ExceptionHandling
#### EntryActionException
01. establish an entry action throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to extensions and then pass to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

#### ExitActionException
01. establish an exit action throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to extensions and then pass to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

#### GuardException
01. establish a guard throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to extensions and then pass to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

#### NoExceptionHandlerRegistered
01. establish an exception throwing state machine without a registered exception handler
02. when an exception occurs
03. should (re-)throw exception

#### StartingException
01. establish a entry action for the initial state that throws an exception
02. when starting the state machine
03. should catch exception and fire transition exception event
04. should pass thrown exception to event arguments of transition exception event

#### TransitionActionException
01. establish a transition action throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to extensions and then pass to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

### ExitActions
#### EventArgument
01. establish a state machine with an exit action taking an event argument
02. when leaving the state
03. it should pass event argument to exit action

#### ExitAction
01. establish a state machine with exit action on a state
02. when leaving the state
03. it should execute the synchronous exit action
04. it should execute the asynchronous exit action

#### ExitActionWithParameter
01. establish a state machine with exit action with parameter on a state
02. when leaving the state
03. it should execute the synchronous exit action
04. it should pass parameter to the synchronous exit action
05. it should execute the asynchronous exit action
06. it should pass parameter to the asynchronous exit action

#### MultipleExitActions
01. establish a state machine with several exit actions on a state
02. when leaving the state
03. it should execute all exit actions

### Extensions
#### BeforeExecutingEntryActions
01. establish an extension
02. establish a state machine using the extension
03. when firing an event onto the state machine
04. it should call EnteringState on registered extensions for target state

#### BeforeExecutingEntryActionsHierarchical
01. establish an extension
02. establish a hierarchical state machine using the extension
03. when firing an event onto the state machine
04. it should call EnteringState on registered extensions for entered super states of target state
05. it should call EnteringState on registered extensions for entered leaf target state

### Guards
#### MatchingGuard
01. establish a state machine with guarded transitions
02. when an event is fired
03. it should take transition guarded with first matching guard

#### NoMatchingGuard
01. establish state machine with no matching guard
02. when an event is fired
03. it should notify about declined transition

#### OtherwiseGuard
01. establish a state machine with otherwise guard and no matching other guard
02. when an event is fired
03. it should_take_transition_guarded_with_otherwise

#### PassingArguments
01. establish a state machine with guarded transitions using an argument
02. when an event is fired
03. it should pass the argument to the sync guards
04. it should pass the argument to the async guards

### HierarchicalStateMachineInitialization
#### InitializationInLeafState
01. establish a hierarchical state machine with leaf state as initial state
02. when starting the state machine
03. it should set current state of state machine to state to which it is initialized
04. it should execute entry action of state to which state machine is initialized
05. it should execute entry action of super states of the state to which state machine is initialized

#### InitializationInSuperState
01. establish a hierarchical state machine with super state as initial state
02. when starting the state machine
03. it should set current state of state machine to initial leaf state of the state to which it is initialized
04. it should execute entry action of super state to which state machine is initialized
05. it should execute entry actions of initial sub states until a leaf state is reached

### HierarchicalTransitions
#### CommonAncestor
01. establish a hierarchical state machine
02. when firing an event resulting in a transition with a common ancestor
03. the state machine should remain inside common ancestor state

#### NoCommonAncestor
01. establish a hierarchical state machine
02. when firing an event resulting in a transition without a common ancestor
03. it should execute exit action of source state
04. it should execute exit action of parents of source state (recursively)
05. it should execute entry action of parents of destination state (recursively)
06. it should execute entry action of destination state
07. it should execute actions from source upwards and then downwards to destination state

### IncompleteConfiguration
#### TransitionActionException
01. establish a StateDefinition without configurations
02. when the state machine is started
03. it should throw an exception, indicating the missing configuration

### Initialization
#### MissingInitialize
01. establish a state machine definition without initialize
02. when building the state machine from the definition
03. it should throw an InvalidOperationException

#### Start
01. establish an initialized state machine
02. when starting the state machine
03. it should set current state of state machine to state to which it is initialized
04. it should execute entry action of state to which state machine is initialized
05. it should notify extensions that it is entering the initial state
06. it should notify extensions that it has entered the initial state
07. it should notify extensions that the state machine is started

### Persisting
#### Loading
01. establish a saved state machine with history
02. when state machine is loaded
03. it should reset current state
04. it should reset all history states of super states
05. it should notify extensions

#### LoadingAnInitializedStateMachine
01. establish an started state machine
02. when state machine is loaded
03. it should throw invalid operation exception

#### LoadingEventsForActiveStateMachine
01. establish a passive state machine
02. when it is loaded with Events
03. it should process those events

#### LoadingEventsForPassiveStateMachine
01. establish a passive state machine
02. when it is loaded with Events
03. it should process those events

#### LoadingNonInitializedStateMachine
01. when a non-initialized state machine is loaded
02. it should not be initialized already

#### SavingEventsForActiveStateMachine
01. establish a state machine
02. when events are fired
03. and it is saved
04. it should save those events

#### SavingEventsForPassiveStateMachine
01. establish a state machine
02. when events are fired
03. and it is saved
04. it should save those events

### Reporting
#### Report
01. establish a state machine
02. establish a state machine reporter
03. when creating a report
04. it should call the passed reporter

### StartStop
#### Starting
01. (Background) establish a state machine
02. establish some queued events
03. when starting
04. it should execute queued events

#### Stopping
01. (Background) establish a state machine
02. establish started state machine
03. when stopping a state machine
04. when firing events onto the state machine
05. it should queue events

### Transitions
#### ExecutingTransition
01. establish a state machine with transitions
02. when firing an event onto the state machine
03. it should execute transition by switching state
04. it should execute synchronous transition actions
05. it should execute asynchronous transition actions
06. it should pass parameters to transition action
07. it should execute synchronous exit action of source state
08. it should execute asynchronous exit action of source state
09. it should execute synchronous entry action of destination state
10. it should execute asynchronous entry action of destination state
11. it should execute entry action of destination state

#### InternalTransition
01. establish a state machine with an internal transition
02. when executing the internal transition
03. it should stay in the same state
04. it should execute transition actions
05. it should not execute exit actions
06. it should not execute entry actions

#### SelfTransition
01. establish a state machine with a self transition
02. when executing the internal transition
03. it should stay in the same state
04. it should execute transition actions
05. it should execute exit actions
06. it should execute entry actions

#### TransitionWithThrowingAction
01. establish a state machine with a transition action that throws an exception
02. when executing the failing transition
03. it should fire the TransitionExceptionThrown event
04. it should still go to the destination state

## Sync
### ActiveStateMachines
#### CustomStateMachineName
01. establish an instantiated active state machine with custom name
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use custom name for state machine

#### DefaultStateMachineName
01. establish an instantiated active state machine
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use the type of the state machine as name for state machine

#### EventsQueueing
01. establish an active state machine with transitions
02. when firing an event onto the state machine
03. it should queue event at the end

#### PriorityEventsQueueing
01. establish an active state machine with transitions
02. when firing a priority event onto the state machine
03. it should queue event at the front

### CustomTypes
#### CustomTypesForStatesAndEvents
01. establish a state machine with custom types for states and events
02. when using the state machine
03. it should use equals to compare states and events

### EntryActions
#### EntryAction
01. establish a state machine with entry action on a state
02. when entering the state
03. it should execute the entry action

#### EntryActionWithParameter
01. establish a state machine with entry action with parameter on a state
02. when entering the state
03. it should execute the entry action
04. it should pass parameter to the entry action

#### EventArgument
01. establish a state machine with an entry action taking an event argument
02. when entering the state
03. it should pass event argument to entry action

#### MultipleEntryActions
01. establish a state machine with several entry actions on a state
02. when entering the state
03. It should execute all entry actions

### ExceptionHandling
#### EntryActionException
01. establish an entry action throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

#### ExitActionException
01. establish an exit action throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

#### GuardException
01. establish a guard throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

#### NoExceptionHandlerRegistered
01. establish an exception throwing state machine without a registered exception handler
02. when an exception occurs
03. should (re-)throw exception

#### StartingException
01. establish a entry action for the initial state that throws an exception
02. when starting the state machine
03. should catch exception and fire transition exception event
04. should pass thrown exception to event arguments of transition exception event

#### TransitionActionException
01. establish a transition action throwing an exception
02. when executing the transition
03. should catch exception and fire transition exception event
04. should pass source state of failing transition to event arguments of transition exception event
05. should pass event id causing transition to event arguments of transition exception event
06. should pass thrown exception to event arguments of transition exception event
07. should pass event parameter to event argument of transition exception event
08. should still go to the destination state

### ExitActions
#### EventArgument
01. establish a state machine with an exit action taking an event argument
02. when leaving the state
03. it should pass event argument to exit action

#### ExitAction
01. establish a state machine with exit action on a state
02. when leaving the state
03. it should execute the exit action

#### ExitActionWithParameter
01. establish a state machine with exit action with parameter on a state
02. when leaving the state
03. it should execute the exit action
04. it should pass parameter to the exit action

#### MultipleExitActions
01. establish a state machine with several exit actions on a state
02. when leaving the state
03. It should execute all exit actions

### Extensions
#### BeforeExecutingEntryActions
01. establish an extension
02. establish a state machine using the extension
03. when firing an event onto the state machine
04. it should call EnteringState on registered extensions for target state

#### BeforeExecutingEntryActionsHierarchical
01. establish an extension
02. establish a hierarchical state machine using the extension
03. when firing an event onto the state machine
04. it should call EnteringState on registered extensions for entered super states of target state
05. it should call EnteringState on registered extensions for entered leaf target state

### Guards
#### MatchingGuard
01. establish a state machine with guarded transitions
02. when an event is fired
03. it should take transition guarded with first matching guard

#### NoMatchingGuard
01. establish state machine with no matching guard
02. when an event is fired
03. it should notify about declined transition

#### OtherwiseGuard
01. establish a state machine with otherwise guard and no matching other guard
02. when an event is fired
03. it should_take_transition_guarded_with_otherwise

#### PassingArguments
01. establish a state machine with guarded transitions using an argument
02. when an event is fired
03. it should pass the argument to the guards

### HierarchicalStateMachineInitialization
#### InitializationInLeafState
01. establish a hierarchical state machine with leaf state as initial state
02. when starting the state machine
03. it should set current state of state machine to state to which it is initialized
04. it should execute entry action of state to which state machine is initialized
05. it should execute entry action of super states of the state to which state machine is initialized

#### InitializationInSuperState
01. establish a hierarchical state machine with super state as initial state
02. when initializing to a super state and starting the state machine
03. it should set current state of state machine to initial leaf state of the state to which it is initialized
04. it should execute entry action of super state to which state machine is initialized
05. it should execute entry actions of initial sub states until a leaf state is reached

### HierarchicalTransitions
#### CommonAncestor
01. establish a hierarchical state machine
02. when firing an event resulting in a transition with a common ancestor
03. the state machine should remain inside common ancestor state

#### NoCommonAncestor
01. establish a hierarchical state machine
02. when firing an event resulting in a transition without a common ancestor
03. it should execute exit action of source state
04. it should execute exit action of parents of source state (recursively)
05. it should execute entry action of parents of destination state (recursively)
06. it should execute entry action of destination state
07. it should execute actions from source upwards and then downwards to destination state

### IncompleteConfiguration
#### TransitionActionException
01. establish a StateDefinition without any state configurations
02. when the state machine is started
03. it should throw an exception, indicating the missing configuration

### Initialization
#### MissingInitialize
01. establish a state machine definition without initialize
02. when building the state machine from the definition
03. it should throw an InvalidOperationException

#### Start
01. establish a state machine
02. when starting the state machine
03. it should set current state of state machine to state to which it is initialized
04. it should execute entry action of state to which state machine is initialized
05. it should notify extensions that it is entering the initial state
06. it should notify extensions that it has entered the initial state
07. it should notify extensions that the state machine is started

### PassiveStateMachines
#### CustomStateMachineName
01. establish an instantiated passive state machine with custom name
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use custom name for state machine

#### DefaultStateMachineName
01. establish an instantiated passive state machine
02. establish a state machine reporter
03. when the state machine report is generated
04. it should use the type of the state machine as name for state machine

#### EventsQueueing
01. establish a passive state machine with transitions
02. when firing an event onto the state machine
03. it should queue event at the end

#### PriorityEventsQueueing
01. establish a passive state machine with transitions
02. when firing a priority event onto the state machine
03. it should queue event at the front

### Persisting
#### Loading
01. establish a saved state machine with history
02. when state machine is loaded
03. it should reset current state
04. it should reset all history states of super states
05. it should notify extensions

#### LoadingAnInitializedStateMachine
01. establish a started state machine
02. when state machine is loaded
03. it should throw invalid operation exception

#### LoadingEvents
01. establish a state machine
02. when it is loaded with Events
03. it should process those events

#### LoadingNonInitializedStateMachine
01. when a not started state machine is loaded
02. it should not be initialized already

#### SavingEvents
01. establish a state machine
02. when events are fired
03. and it is saved
04. it should save those events

### Reporting
#### Report
01. establish a state machine
02. establish a state machine reporter
03. when creating a report
04. it should call the passed reporter

### StartStop
#### Starting
01. (Background) establish a state machine
02. establish some queued events
03. when starting
04. it should execute queued events

#### Stopping
01. (Background) establish a state machine
02. establish started state machine
03. when stopping a state machine
04. when firing events onto the state machine
05. it should queue events

### StateMachineExtensions
#### AddingExtensions
01. establish a state machine
02. when adding an extension
03. it should notify extension about internal events

#### ClearingExtensions
01. establish a state machine with an extension
02. when clearing all extensions from the state machine
03. it should not anymore notify extension about internal events

### Transitions
#### ExecutingTransition
01. establish a state machine with transitions
02. when firing an event onto the state machine
03. it should execute the transition by switching the state
04. it should execute transition actions
05. it should pass parameters to transition action
06. it should execute exit action of source state
07. it should execute entry action of destination state

#### InternalTransition
01. establish a state machine with an internal transition
02. when executing the internal transition
03. it should stay in the same state
04. it should execute transition actions
05. it should not execute exit actions
06. it should not execute entry actions

#### SelfTransition
01. establish a state machine with a self transition
02. when executing the internal transition
03. it should stay in the same state
04. it should execute transition actions
05. it should execute exit actions
06. it should execute entry actions

#### TransitionWithThrowingAction
01. establish a state machine with a transition action that throws an exception
02. when executing the failing transition
03. it should fire the TransitionExceptionThrown event
04. it should still go to the destination state