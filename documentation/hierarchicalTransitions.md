# Hierarchical Transitions
The current state of the state machine is always a leaf state (= a state without children). Whenever a transition points to a super state (= a state with children), the state machine enters a child state depending on the specified history type and initial child state. This is repeated until a leaf state is reached.

## Resolution of Transitions defined up in the Hierarchy
When an event is fired on the state machine, the current state (remember that this is a leaf state) is checked whether it defines a transition for this event. If a transition is found, it is executed. If no transition is found, the state hierarchy is traversed from the current state up until a parent state is found that defines a transition for the event or the root state is reached and no matching transition is found. If a transition is found, it is executed. If no matching transition is found, a `TransitionDeclined` event is fired by the state machine.

## History Type
The `HistoryType`, which is set when a hierarchy is defined, specifies how the state machine enters child states when a transition ends in a super state.

### None
The state enters into its initial sub-state. The sub-state itself enters its initial sub-state and so on until the innermost nested state is reached.

### Shallow
The state enters into its last active sub-state. The sub-state itself enters its initial sub-state and so on until the innermost nested state is reached. If the state was never entered bevor, the initial sub-state is entered.

### Deep
The state enters into its last active sub-state. The sub-state itself enters into its last active sub-state and so on until the innermost nested state is reached.

The state in that the transition ends defines the history behaviour of all sub states. That means if the state defines a history type equal to `Deep`, the history types of the entered children are ignored, the state machine will end in the last active state of the super state the transition ended in.

## Entering and Leaving States and Executing Actions
The first step in executing a transition is leaving states. States are left beginning with the current state up the hierarchy until the destination state of the transition is a child of the state currently inspected.

The second step is executing all actions defined by the transition.

The third step is entering states. From the common super state of the source and destination states, states are entered down the hierarchy until the destination state is reached.

## Internal Transitions
No states are left or entered when an internal transition is executed. An internal transition is a transition with no destination specified; the transition is executed inside a state (leaf state or super state).

## Examples
Let's take a simple elevator state machine (see [StateMachine sample](example.md) for the definition in code):

### Resolving Transitions
When the elevator is moving up (current state = `MovingUp`) and the event `Stop` is fired, the `MovingUp` state is searched for a matching transition and none is found. Therefore the next state up in the hierarchy (`Moving`) is checked. There, a transition for the event `Stop` is found and the transition is executed.

### Entering Sub States
When the elevator is in the state `Error` and the `Reset` event is fired, the state machine enters the last active state because `Deep` is used as `HistoryType` of the `Healthy` state (a very risky implementation of an elevator by the way ;-) )