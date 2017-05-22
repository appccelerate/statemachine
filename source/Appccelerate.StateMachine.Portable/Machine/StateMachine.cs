//-------------------------------------------------------------------------------
// <copyright file="StateMachine.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using Appccelerate.StateMachine.Machine.Events;
    using Appccelerate.StateMachine.Persistence;
    using Appccelerate.StateMachine.Syntax;

    /// <summary>
    /// Base implementation of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachine<TState, TEvent> : 
        INotifier<TState, TEvent>, 
        IStateMachineInformation<TState, TEvent>,
        IExtensionHost<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionary<TState, TEvent> states;
        private readonly IFactory<TState, TEvent> factory;
        private readonly Initializable<TState> initialStateId;
        private readonly string name;
        private readonly List<IExtension<TState, TEvent>> extensions;
        private IState<TState, TEvent> currentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        public StateMachine()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of this state machine used in log messages.</param>
        public StateMachine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of this state machine used in log messages.</param>
        /// <param name="factory">The factory used to create internal instances.</param>
        public StateMachine(string name, IFactory<TState, TEvent> factory)
        {
            this.name = name;
            this.factory = factory ?? new StandardFactory<TState, TEvent>(this, this);
            this.states = new StateDictionary<TState, TEvent>(this.factory);
            this.extensions = new List<IExtension<TState, TEvent>>();

            this.initialStateId = new Initializable<TState>();
        }

        /// <summary>
        /// Occurs when no transition could be executed.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined;

        /// <summary>
        /// Occurs when an exception was thrown inside a transition of the state machine.
        /// </summary>
        public event EventHandler<TransitionExceptionEventArgs<TState, TEvent>> TransitionExceptionThrown;

        /// <summary>
        /// Occurs when a transition begins.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionBegin;

        /// <summary>
        /// Occurs when a transition completed.
        /// </summary>
        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted;

        /// <summary>
        /// Gets the name of this instance.
        /// </summary>
        /// <value>The name of this instance.</value>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the id of the current state.
        /// </summary>
        /// <value>The id of the current state.</value>
        public TState CurrentStateId
        {
            get { return this.CurrentState.Id; }
        }

        /// <summary>
        /// Gets or sets the state of the current.
        /// </summary>
        /// <value>The state of the current.</value>
        private IState<TState, TEvent> CurrentState
        {
            get
            {
                this.CheckThatStateMachineIsInitialized();
                this.CheckThatStateMachineHasEnteredInitialState();

                return this.currentState;
            }

            set
            {
                IState<TState, TEvent> oldState = this.currentState;

                this.currentState = value;

                this.extensions.ForEach(extension => extension.SwitchedState(this, oldState, this.currentState));
            }
        }

        /// <summary>
        /// Adds the <paramref name="extension"/>.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void AddExtension(IExtension<TState, TEvent> extension)
        {
            this.extensions.Add(extension);
        }

        /// <summary>
        /// Clears all extensions.
        /// </summary>
        public void ClearExtensions()
        {
            this.extensions.Clear();
        }

        /// <summary>
        /// Executes the specified <paramref name="action"/> for all extensions.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void ForEach(Action<IExtension<TState, TEvent>> action)
        {
            this.extensions.ForEach(action);
        }

        /// <summary>
        /// Define the behavior of a state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Syntax to build state behavior.</returns>
        public IEntryActionSyntax<TState, TEvent> In(TState state)
        {
            IState<TState, TEvent> newState = this.states[state];

            return new StateBuilder<TState, TEvent>(newState, this.states, this.factory);
        }

        /// <summary>
        /// Initializes the state machine by setting the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state of the state machine.</param>
        public void Initialize(TState initialState)
        {
            this.extensions.ForEach(extension => extension.InitializingStateMachine(this, ref initialState));

            this.Initialize(this.states[initialState]);

            this.extensions.ForEach(extension => extension.InitializedStateMachine(this, initialState));
        }

        /// <summary>
        /// Enters the initial state that was previously set with <see cref="Initialize(TState)"/>.
        /// </summary>
        public void EnterInitialState()
        {
            this.CheckThatStateMachineIsInitialized();

            this.extensions.ForEach(extension => extension.EnteringInitialState(this, this.initialStateId.Value));

            var context = this.factory.CreateTransitionContext(null, new Missable<TEvent>(), Missing.Value, this);
            this.EnterInitialState(this.states[this.initialStateId.Value], context);

            this.extensions.ForEach(extension => extension.EnteredInitialState(this, this.initialStateId.Value, context));
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        public void Fire(TEvent eventId)
        {
            this.Fire(eventId, Missing.Value);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public void Fire(TEvent eventId, object eventArgument)
        {
            this.CheckThatStateMachineIsInitialized();
            this.CheckThatStateMachineHasEnteredInitialState();

            this.extensions.ForEach(extension => extension.FiringEvent(this, ref eventId, ref eventArgument));

            ITransitionContext<TState, TEvent> context = this.factory.CreateTransitionContext(this.CurrentState, new Missable<TEvent>(eventId), eventArgument, this);
            ITransitionResult<TState, TEvent> result = this.CurrentState.Fire(context);

            if (!result.Fired)
            {
                this.OnTransitionDeclined(context);
                return;
            }

            this.CurrentState = result.NewState;

            this.extensions.ForEach(extension => extension.FiredEvent(this, context));

            this.OnTransitionCompleted(context);
        }

        /// <summary>
        /// Defines the hierarchy on.
        /// </summary>
        /// <param name="superStateId">The super state id.</param>
        /// <returns>Syntax to build a state hierarchy.</returns>
        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return new HierarchyBuilder<TState, TEvent>(this.states, superStateId);
        }

        public void OnExceptionThrown(ITransitionContext<TState, TEvent> context, Exception exception)
        {
            RethrowExceptionIfNoHandlerRegistered(exception, this.TransitionExceptionThrown);

            this.RaiseEvent(this.TransitionExceptionThrown, new TransitionExceptionEventArgs<TState, TEvent>(context, exception), context, false);
        }

        /// <summary>
        /// Fires the <see cref="TransitionBegin"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        public void OnTransitionBegin(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionBegin, new TransitionEventArgs<TState, TEvent>(transitionContext), transitionContext, true);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.name ?? base.ToString();
        }

        /// <summary>
        /// Creates a report with the specified generator.
        /// </summary>
        /// <param name="reportGenerator">The report generator.</param>
        public void Report(IStateMachineReport<TState, TEvent> reportGenerator)
        {
            Guard.AgainstNullArgument("reportGenerator", reportGenerator);

            reportGenerator.Report(this.ToString(), this.states.GetStates(), this.initialStateId);
        }

        public void Save(IStateMachineSaver<TState> stateMachineSaver)
        {
            Guard.AgainstNullArgument("stateMachineSaver", stateMachineSaver);

            stateMachineSaver.SaveCurrentState(this.currentState != null ? 
                new Initializable<TState> { Value = this.currentState.Id } : 
                new Initializable<TState>());

            IEnumerable<IState<TState, TEvent>> superStatesWithLastActiveState = this.states.GetStates()
                .Where(s => s.SubStates.Any())
                .Where(s => s.LastActiveState != null)
                .ToList();

            var historyStates = superStatesWithLastActiveState.ToDictionary(
                s => s.Id,
                s => s.LastActiveState.Id);

            stateMachineSaver.SaveHistoryStates(historyStates);
        }

        public void Load(IStateMachineLoader<TState> stateMachineLoader)
        {
            Guard.AgainstNullArgument("stateMachineLoader", stateMachineLoader);
            this.CheckThatStateMachineIsNotAlreadyInitialized();

            this.LoadCurrentState(stateMachineLoader);
            this.LoadHistoryStates(stateMachineLoader);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void RethrowExceptionIfNoHandlerRegistered<T>(Exception exception, EventHandler<T> exceptionHandler) where T : EventArgs
        {
            if (exceptionHandler == null)
            {
                throw new StateMachineException("No exception listener is registered. Exception: ", exception);
            }
        }

        /// <summary>
        /// Fires the <see cref="TransitionDeclined"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition event context.</param>
        private void OnTransitionDeclined(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionDeclined, new TransitionEventArgs<TState, TEvent>(transitionContext), transitionContext, true);
        }

        /// <summary>
        /// Fires the <see cref="TransitionCompleted"/> event.
        /// </summary>
        /// <param name="transitionContext">The transition event context.</param>
        private void OnTransitionCompleted(ITransitionContext<TState, TEvent> transitionContext)
        {
            this.RaiseEvent(this.TransitionCompleted, new TransitionCompletedEventArgs<TState, TEvent>(this.CurrentStateId, transitionContext), transitionContext, true);
        }

        private void LoadCurrentState(IStateMachineLoader<TState> stateMachineLoader)
        {
            Initializable<TState> loadedCurrentState = stateMachineLoader.LoadCurrentState();

            this.currentState = loadedCurrentState.IsInitialized ? this.states[loadedCurrentState.Value] : null;
        }

        private void LoadHistoryStates(IStateMachineLoader<TState> stateMachineLoader)
        {
            IDictionary<TState, TState> historyStates = stateMachineLoader.LoadHistoryStates();
            foreach (KeyValuePair<TState, TState> historyState in historyStates)
            {
                IState<TState, TEvent> superState = this.states[historyState.Key];
                IState<TState, TEvent> lastActiveState = this.states[historyState.Value];

                if (!superState.SubStates.Contains(lastActiveState))
                {
                    throw new InvalidOperationException(ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
                }

                superState.LastActiveState = lastActiveState;
            }
        }

        /// <summary>
        /// Initializes the state machine by setting the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        private void Initialize(IState<TState, TEvent> initialState)
        {
            if (this.initialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }

            this.initialStateId.Value = initialState.Id;
        }

        private void EnterInitialState(IState<TState, TEvent> initialState, ITransitionContext<TState, TEvent> context)
        {
            var initializer = this.factory.CreateStateMachineInitializer(initialState, context);
            this.CurrentState = initializer.EnterInitialState();
        }

        private void RaiseEvent<T>(EventHandler<T> eventHandler, T arguments, ITransitionContext<TState, TEvent> context, bool raiseEventOnException) where T : EventArgs
        {
            try
            {
                if (eventHandler == null)
                {
                    return;
                }

                eventHandler(this, arguments);
            }
            catch (Exception e)
            {
                if (!raiseEventOnException)
                {
                    throw;
                }

                ((INotifier<TState, TEvent>)this).OnExceptionThrown(context, e);
            }
        }

        private void CheckThatStateMachineIsInitialized()
        {
            if (this.currentState == null && !this.initialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineNotInitialized);
            }
        }

        private void CheckThatStateMachineIsNotAlreadyInitialized()
        {
            if (this.currentState != null || this.initialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }
        }

        private void CheckThatStateMachineHasEnteredInitialState()
        {
            if (this.currentState == null)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineHasNotYetEnteredInitialState);
            }
        }
    }
}