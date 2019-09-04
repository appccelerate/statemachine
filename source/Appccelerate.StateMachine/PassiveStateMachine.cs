//-------------------------------------------------------------------------------
// <copyright file="PassiveStateMachine.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Machine;
    using Machine.Events;
    using Machine.States;
    using Persistence;

    /// <summary>
    /// A passive state machine.
    /// This state machine reacts to events on the current thread.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class PassiveStateMachine<TState, TEvent> :
        IStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// The internal state machine.
        /// </summary>
        private readonly StateMachine<TState, TEvent> stateMachine;

        /// <summary>
        /// List of all queued events.
        /// </summary>
        private readonly LinkedList<EventInformation<TEvent>> events;

        private readonly StateContainer<TState, TEvent> stateContainer;

        private readonly IReadOnlyDictionary<TState, StateDefinition<TState, TEvent>> stateDefinitions;

        /// <summary>
        /// Whether the state machine is initialized.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// Whether this state machine is executing an event. Allows that events can be added while executing.
        /// </summary>
        private bool executing;

        private bool pendingInitialization;

        public PassiveStateMachine(
            StateMachine<TState, TEvent> stateMachine,
            StateContainer<TState, TEvent> stateContainer,
            IReadOnlyDictionary<TState, StateDefinition<TState, TEvent>> stateDefinitions)
        {
            this.stateMachine = stateMachine;
            this.stateContainer = stateContainer;
            this.stateDefinitions = stateDefinitions;
            this.events = new LinkedList<EventInformation<TEvent>>();
        }

        /// <summary>
        /// Occurs when no transition could be executed.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined
        {
            add => this.stateMachine.TransitionDeclined += value;
            remove => this.stateMachine.TransitionDeclined -= value;
        }

        /// <summary>
        /// Occurs when an exception was thrown inside a transition of the state machine.
        /// </summary>
        public event EventHandler<TransitionExceptionEventArgs<TState, TEvent>> TransitionExceptionThrown
        {
            add => this.stateMachine.TransitionExceptionThrown += value;
            remove => this.stateMachine.TransitionExceptionThrown -= value;
        }

        /// <summary>
        /// Occurs when a transition begins.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionBegin
        {
            add => this.stateMachine.TransitionBegin += value;
            remove => this.stateMachine.TransitionBegin -= value;
        }

        /// <summary>
        /// Occurs when a transition completed.
        /// </summary>
        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted
        {
            add => this.stateMachine.TransitionCompleted += value;
            remove => this.stateMachine.TransitionCompleted -= value;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running. The state machine is running if if was started and not yet stopped.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public bool IsRunning
        {
            get; private set;
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        public void Fire(TEvent eventId)
        {
            this.Fire(eventId, null);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public void Fire(TEvent eventId, object eventArgument)
        {
            this.events.AddLast(new EventInformation<TEvent>(eventId, eventArgument));

            this.stateContainer.ForEach(extension => extension.EventQueued(this.stateContainer, eventId, eventArgument));

            this.Execute();
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        public void FirePriority(TEvent eventId)
        {
            this.FirePriority(eventId, null);
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public void FirePriority(TEvent eventId, object eventArgument)
        {
            this.events.AddFirst(new EventInformation<TEvent>(eventId, eventArgument));

            this.stateContainer.ForEach(extension => extension.EventQueuedWithPriority(this.stateContainer, eventId, eventArgument));

            this.Execute();
        }

        /// <summary>
        /// Initializes the state machine to the specified initial state.
        /// </summary>
        /// <param name="initialState">The state to which the state machine is initialized.</param>
        public void Initialize(TState initialState)
        {
            this.CheckThatNotAlreadyInitialized();

            this.initialized = true;
            this.pendingInitialization = true;

            this.stateMachine.Initialize(initialState, this.stateContainer, this.stateContainer);
        }

        /// <summary>
        /// Starts the state machine. Events will be processed.
        /// If the state machine is not started then the events will be queued until the state machine is started.
        /// Already queued events are processed.
        /// </summary>
        public void Start()
        {
            this.CheckThatStateMachineIsInitialized();

            this.IsRunning = true;

            this.stateContainer.ForEach(extension => extension.StartedStateMachine(this.stateContainer));

            this.Execute();
        }

        /// <summary>
        /// Creates a state machine report with the specified generator.
        /// </summary>
        /// <param name="reportGenerator">The report generator.</param>
        public void Report(IStateMachineReport<TState, TEvent> reportGenerator)
        {
            reportGenerator.Report(this.ToString(), this.stateDefinitions.Values, this.stateContainer.InitialStateId);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.stateContainer.Name ?? this.GetType().FullName;
        }

        /// <summary>
        /// Stops the state machine. Events will be queued until the state machine is started.
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;

            this.stateContainer.ForEach(extension => extension.StoppedStateMachine(this.stateContainer));
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void AddExtension(IExtension<TState, TEvent> extension)
        {
            this.stateContainer.Extensions.Add(extension);
        }

        /// <summary>
        /// Clears all extensions.
        /// </summary>
        public void ClearExtensions()
        {
            this.stateContainer.Extensions.Clear();
        }

        /// <summary>
        /// Saves the current state and history states to a persisted state. Can be restored using <see cref="Load"/>.
        /// </summary>
        /// <param name="stateMachineSaver">Data to be persisted is passed to the saver.</param>
        public void Save(IStateMachineSaver<TState> stateMachineSaver)
        {
            Guard.AgainstNullArgument(nameof(stateMachineSaver), stateMachineSaver);

            stateMachineSaver.SaveCurrentState(this.stateContainer.CurrentState != null ?
                new Initializable<TState> { Value = this.stateContainer.CurrentState.Id } :
                new Initializable<TState>());

            var historyStates = this.stateContainer
                .LastActiveStates
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Id);

            stateMachineSaver.SaveHistoryStates(historyStates);
        }

        /// <summary>
        /// Loads the current state and history states from a persisted state (<see cref="Save"/>).
        /// The loader should return exactly the data that was passed to the saver.
        /// </summary>
        /// <param name="stateMachineLoader">Loader providing persisted data.</param>
        public void Load(IStateMachineLoader<TState> stateMachineLoader)
        {
            Guard.AgainstNullArgument(nameof(stateMachineLoader), stateMachineLoader);

            this.CheckThatNotAlreadyInitialized();
            this.CheckThatStateMachineIsNotAlreadyInitialized();

            var loadedCurrentState = stateMachineLoader.LoadCurrentState();
            var historyStates = stateMachineLoader.LoadHistoryStates();

            var wasSuccessful = SetCurrentState();
            LoadHistoryStates();
            NotifyExtensions();

            this.initialized = wasSuccessful;

            bool SetCurrentState()
            {
                if (loadedCurrentState.IsInitialized)
                {
                    this.stateContainer.CurrentState = this.stateDefinitions[loadedCurrentState.Value];
                    return true;
                }

                this.stateContainer.CurrentState = null;
                return false;
            }

            void LoadHistoryStates()
            {
                foreach (var historyState in historyStates)
                {
                    var superState = this.stateDefinitions[historyState.Key];
                    var lastActiveState = this.stateDefinitions[historyState.Value];

                    if (!superState.SubStates.Contains(lastActiveState))
                    {
                        throw new InvalidOperationException(ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
                    }

                    this.stateContainer.SetLastActiveStateFor(superState.Id, lastActiveState);
                }
            }

            void NotifyExtensions()
            {
                this.stateContainer.Extensions.ForEach(
                    extension => extension.Loaded(
                        this.stateContainer,
                        loadedCurrentState,
                        historyStates));
            }
        }

        private void CheckThatNotAlreadyInitialized()
        {
            if (this.initialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }
        }

        private void CheckThatStateMachineIsInitialized()
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineNotInitialized);
            }
        }

        private void CheckThatStateMachineIsNotAlreadyInitialized()
        {
            if (this.stateContainer.CurrentState != null || this.stateContainer.InitialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }
        }

        /// <summary>
        /// Executes all queued events.
        /// </summary>
        private void Execute()
        {
            if (this.executing || !this.IsRunning)
            {
                return;
            }

            this.executing = true;
            try
            {
                this.ProcessQueuedEvents();
            }
            finally
            {
                this.executing = false;
            }
        }

        /// <summary>
        /// Processes the queued events.
        /// </summary>
        private void ProcessQueuedEvents()
        {
            this.InitializeStateMachineIfInitializationIsPending();

            while (this.events.Count > 0)
            {
                var eventToProcess = this.GetNextEventToProcess();
                this.FireEventOnStateMachine(eventToProcess);
            }
        }

        private void InitializeStateMachineIfInitializationIsPending()
        {
            if (!this.pendingInitialization)
            {
                return;
            }

            this.stateMachine.EnterInitialState(this.stateContainer, this.stateContainer, this.stateDefinitions);

            this.pendingInitialization = false;
        }

        /// <summary>
        /// Gets the next event to process for the queue.
        /// </summary>
        /// <returns>The next queued event.</returns>
        private EventInformation<TEvent> GetNextEventToProcess()
        {
            var e = this.events.First.Value;
            this.events.RemoveFirst();
            return e;
        }

        /// <summary>
        /// Fires the event on state machine.
        /// </summary>
        /// <param name="e">The event to fire.</param>
        private void FireEventOnStateMachine(EventInformation<TEvent> e)
        {
            this.stateMachine.Fire(e.EventId, e.EventArgument, this.stateContainer, this.stateContainer, this.stateDefinitions);
        }
    }
}