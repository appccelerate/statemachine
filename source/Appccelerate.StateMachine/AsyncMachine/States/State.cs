//-------------------------------------------------------------------------------
// <copyright file="State.cs" company="Appccelerate">
//   Copyright (c) 2008-2019 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.AsyncMachine.States
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Appccelerate.StateMachine.AsyncMachine.ActionHolders;
    using Appccelerate.StateMachine.AsyncMachine.Transitions;

    /// <summary>
    /// A state of the state machine.
    /// A state can be a sub-state or super-state of another state.
    /// </summary>
    /// <typeparam name="TState">The type of the state id.</typeparam>
    /// <typeparam name="TEvent">The type of the event id.</typeparam>
    public class State<TState, TEvent>
        : IState<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Collection of the sub-states of this state.
        /// </summary>
        private readonly List<IState<TState, TEvent>> subStates;

        /// <summary>
        /// Collection of transitions that start in this state (<see cref="ITransition{TState,TEvent}.Source"/> is equal to this state).
        /// </summary>
        private readonly TransitionDictionary<TState, TEvent> transitions;

        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;

        private readonly IExtensionHost<TState, TEvent> extensionHost;

        /// <summary>
        /// The level of this state within the state hierarchy [1..maxLevel].
        /// </summary>
        private int level;

        /// <summary>
        /// The super-state of this state. Null for states with <see cref="level"/> equal to 1.
        /// </summary>
        private IState<TState, TEvent> superState;

        /// <summary>
        /// The initial sub-state of this state.
        /// </summary>
        private IState<TState, TEvent> initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="State&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="id">The unique id of this state.</param>
        /// <param name="stateMachineInformation">The state machine information.</param>
        /// <param name="extensionHost">The extension host.</param>
        public State(TState id, IStateMachineInformation<TState, TEvent> stateMachineInformation, IExtensionHost<TState, TEvent> extensionHost)
        {
            this.Id = id;
            this.level = 1;
            this.stateMachineInformation = stateMachineInformation;
            this.extensionHost = extensionHost;

            this.subStates = new List<IState<TState, TEvent>>();
            this.transitions = new TransitionDictionary<TState, TEvent>(this);

            this.EntryActions = new List<IActionHolder>();
            this.ExitActions = new List<IActionHolder>();
        }

        /// <summary>
        /// Gets or sets the last active state of this state.
        /// </summary>
        /// <value>The last state of the active.</value>
        public IState<TState, TEvent> LastActiveState { get; set; }

        /// <summary>
        /// Gets the unique id of this state.
        /// </summary>
        /// <value>The id of this state.</value>
        public TState Id { get; }

        /// <summary>
        /// Gets the entry actions.
        /// </summary>
        /// <value>The entry actions.</value>
        public IList<IActionHolder> EntryActions { get; }

        /// <summary>
        /// Gets the exit actions.
        /// </summary>
        /// <value>The exit action.</value>
        public IList<IActionHolder> ExitActions { get; }

        /// <summary>
        /// Gets or sets the initial sub state of this state.
        /// </summary>
        /// <value>The initial sub state of this state.</value>
        public IState<TState, TEvent> InitialState
        {
            get => this.initialState;

            set
            {
                this.CheckInitialStateIsNotThisInstance(value);
                this.CheckInitialStateIsASubState(value);

                this.initialState = this.LastActiveState = value;
            }
        }

        /// <summary>
        /// Gets or sets the super-state of this state.
        /// </summary>
        /// <remarks>
        /// The <see cref="Level"/> of this state is changed accordingly to the super-state.
        /// </remarks>
        /// <value>The super-state of this super.</value>
        public IState<TState, TEvent> SuperState
        {
            get => this.superState;

            set
            {
                this.CheckSuperStateIsNotThisInstance(value);

                this.superState = value;

                this.SetInitialLevel();
            }
        }

        /// <summary>
        /// Gets or sets the level of this state in the state hierarchy.
        /// When set then the levels of all sub-states are changed accordingly.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get => this.level;

            set
            {
                this.level = value;

                this.SetLevelOfSubStates();
            }
        }

        /// <summary>
        /// Gets or sets the history type of this state.
        /// </summary>
        /// <value>The type of the history.</value>
        public HistoryType HistoryType { get; set; } = HistoryType.None;

        /// <summary>
        /// Gets the sub-states of this state.
        /// </summary>
        /// <value>The sub-states of this state.</value>
        public ICollection<IState<TState, TEvent>> SubStates => this.subStates;

        /// <summary>
        /// Gets the transitions that start in this state.
        /// </summary>
        /// <value>The transitions.</value>
        public ITransitionDictionary<TState, TEvent> Transitions => this.transitions;

        /// <summary>
        /// Goes recursively up the state hierarchy until a state is found that can handle the event.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The result of the transition.</returns>
        public async Task<ITransitionResult<TState, TEvent>> Fire(ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            ITransitionResult<TState, TEvent> result = TransitionResult<TState, TEvent>.NotFired;

            var transitionsForEvent = this.transitions[context.EventId.Value];
            if (transitionsForEvent != null)
            {
                foreach (ITransition<TState, TEvent> transition in transitionsForEvent)
                {
                    result = await transition.Fire(context).ConfigureAwait(false);
                    if (result.Fired)
                    {
                        return result;
                    }
                }
            }

            if (this.SuperState != null)
            {
                result = await this.SuperState.Fire(context).ConfigureAwait(false);
            }

            return result;
        }

        public async Task Entry(ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(this.Id, RecordType.Enter);

            await this.ExecuteEntryActions(context).ConfigureAwait(false);
        }

        public async Task Exit(ITransitionContext<TState, TEvent> context)
        {
            Guard.AgainstNullArgument("context", context);

            context.AddRecord(this.Id, RecordType.Exit);

            await this.ExecuteExitActions(context).ConfigureAwait(false);
            this.SetThisStateAsLastStateOfSuperState();
        }

        public async Task<IState<TState, TEvent>> EnterByHistory(ITransitionContext<TState, TEvent> context)
        {
            IState<TState, TEvent> result = this;

            switch (this.HistoryType)
            {
                case HistoryType.None:
                    result = await this.EnterHistoryNone(context).ConfigureAwait(false);
                    break;

                case HistoryType.Shallow:
                    result = await this.EnterHistoryShallow(context).ConfigureAwait(false);
                    break;

                case HistoryType.Deep:
                    result = await this.EnterHistoryDeep(context).ConfigureAwait(false);
                    break;
            }

            return result;
        }

        public async Task<IState<TState, TEvent>> EnterShallow(ITransitionContext<TState, TEvent> context)
        {
            await this.Entry(context).ConfigureAwait(false);

            return this.initialState == null ?
                        this :
                        await this.initialState.EnterShallow(context).ConfigureAwait(false);
        }

        public async Task<IState<TState, TEvent>> EnterDeep(ITransitionContext<TState, TEvent> context)
        {
            await this.Entry(context).ConfigureAwait(false);

            return this.LastActiveState == null ?
                        this :
                        await this.LastActiveState.EnterDeep(context).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        private static void HandleException(Exception exception, ITransitionContext<TState, TEvent> context)
        {
            context.OnExceptionThrown(exception);
        }

        /// <summary>
        /// Sets the initial level depending on the level of the super state of this instance.
        /// </summary>
        private void SetInitialLevel()
        {
            this.Level = this.superState?.Level + 1 ?? 1;
        }

        /// <summary>
        /// Sets the level of all sub states.
        /// </summary>
        private void SetLevelOfSubStates()
        {
            foreach (var state in this.subStates)
            {
                state.Level = this.level + 1;
            }
        }

        private async Task ExecuteEntryActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in this.EntryActions)
            {
                await this.ExecuteEntryAction(actionHolder, context)
                    .ConfigureAwait(false);
            }
        }

        private async Task ExecuteEntryAction(IActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            try
            {
                await actionHolder
                    .Execute(context.EventArgument)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await this.HandleEntryActionException(context, exception)
                    .ConfigureAwait(false);
            }
        }

        private async Task HandleEntryActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandlingEntryActionException(
                        this.stateMachineInformation, this, context, ref exception))
                .ConfigureAwait(false);

            HandleException(exception, context);

            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandledEntryActionException(
                        this.stateMachineInformation, this, context, exception))
                .ConfigureAwait(false);
        }

        private async Task ExecuteExitActions(ITransitionContext<TState, TEvent> context)
        {
            foreach (var actionHolder in this.ExitActions)
            {
                await this.ExecuteExitAction(actionHolder, context)
                    .ConfigureAwait(false);
            }
        }

        private async Task ExecuteExitAction(IActionHolder actionHolder, ITransitionContext<TState, TEvent> context)
        {
            try
            {
                await actionHolder
                    .Execute(context.EventArgument)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await this.HandleExitActionException(context, exception)
                    .ConfigureAwait(false);
            }
        }

        private async Task HandleExitActionException(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandlingExitActionException(
                        this.stateMachineInformation, this, context, ref exception))
                .ConfigureAwait(false);

            HandleException(exception, context);

            await this.extensionHost
                .ForEach(
                    extension =>
                    extension.HandledExitActionException(
                        this.stateMachineInformation, this, context, exception))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets this instance as the last state of this instance's super state.
        /// </summary>
        private void SetThisStateAsLastStateOfSuperState()
        {
            if (this.superState != null)
            {
                this.superState.LastActiveState = this;
            }
        }

        private async Task<IState<TState, TEvent>> EnterHistoryDeep(ITransitionContext<TState, TEvent> context)
        {
            return this.LastActiveState != null
                       ?
                           await this.LastActiveState.EnterDeep(context)
                               .ConfigureAwait(false)
                       :
                           this;
        }

        private async Task<IState<TState, TEvent>> EnterHistoryShallow(ITransitionContext<TState, TEvent> context)
        {
            return this.LastActiveState != null
                       ?
                           await this.LastActiveState.EnterShallow(context)
                               .ConfigureAwait(false)
                       :
                           this;
        }

        private async Task<IState<TState, TEvent>> EnterHistoryNone(ITransitionContext<TState, TEvent> context)
        {
            return this.initialState != null
                       ?
                           await this.initialState.EnterShallow(context)
                               .ConfigureAwait(false)
                       :
                           this;
        }

        /// <summary>
        /// Throws an exception if the new super state is this instance.
        /// </summary>
        /// <param name="newSuperState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckSuperStateIsNotThisInstance(IState<TState, TEvent> newSuperState)
        {
            if (this == newSuperState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeItsOwnSuperState(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is this instance.
        /// </summary>
        /// <param name="newInitialState">The value.</param>
        // ReSharper disable once UnusedParameter.Local
        private void CheckInitialStateIsNotThisInstance(IState<TState, TEvent> newInitialState)
        {
            if (this == newInitialState)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialSubStateToItself(this.ToString()));
            }
        }

        /// <summary>
        /// Throws an exception if the new initial state is not a sub-state of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        private void CheckInitialStateIsASubState(IState<TState, TEvent> value)
        {
            if (value.SuperState != this)
            {
                throw new ArgumentException(StatesExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(value.ToString(), this.ToString()));
            }
        }
    }
}