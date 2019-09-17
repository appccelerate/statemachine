// <copyright file="ActiveStateMachine.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using Machine;
    using Machine.Events;
    using Persistence;

    /// <summary>
    /// An active state machine.
    /// This state machine reacts to events on its own worker thread and the <see cref="Fire(TEvent,object)"/> or
    /// <see cref="FirePriority(TEvent,object)"/> methods return immediately back to the caller.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class ActiveStateMachine<TState, TEvent> :
        IStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StateMachine<TState, TEvent> stateMachine;
        private readonly StateContainer<TState, TEvent> stateContainer;
        private readonly IStateDefinitionDictionary<TState, TEvent> stateDefinitions;
        private readonly TState initialState;

        private Task worker;
        private CancellationTokenSource stopToken;

        public ActiveStateMachine(
            StateMachine<TState, TEvent> stateMachine,
            StateContainer<TState, TEvent> stateContainer,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions,
            TState initialState)
        {
            this.stateMachine = stateMachine;
            this.stateContainer = stateContainer;
            this.stateDefinitions = stateDefinitions;
            this.initialState = initialState;
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
        public bool IsRunning => this.worker != null && !this.worker.IsCompleted;

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
            lock (this.stateContainer.Events)
            {
                this.stateContainer.Events.AddLast(new EventInformation<TEvent>(eventId, eventArgument));
                Monitor.Pulse(this.stateContainer.Events);
            }

            this.stateContainer.ForEach(extension => extension.EventQueued(eventId, eventArgument));
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
            lock (this.stateContainer.Events)
            {
                this.stateContainer.Events.AddFirst(new EventInformation<TEvent>(eventId, eventArgument));
                Monitor.Pulse(this.stateContainer.Events);
            }

            this.stateContainer.ForEach(extension => extension.EventQueuedWithPriority(eventId, eventArgument));
        }

        /// <summary>
        /// Saves the current state and history states to a persisted state. Can be restored using <see cref="Load"/>.
        /// </summary>
        /// <param name="stateMachineSaver">Data to be persisted is passed to the saver.</param>
        public void Save(IStateMachineSaver<TState, TEvent> stateMachineSaver)
        {
            Guard.AgainstNullArgument(nameof(stateMachineSaver), stateMachineSaver);

            stateMachineSaver.SaveCurrentState(this.stateContainer.CurrentStateId);
            stateMachineSaver.SaveHistoryStates(this.stateContainer.LastActiveStates);
            stateMachineSaver.SaveEvents(this.stateContainer.SaveableEvents);
        }

        /// <summary>
        /// Loads the current state and history states from a persisted state (<see cref="Save"/>).
        /// The loader should return exactly the data that was passed to the saver.
        /// </summary>
        /// <param name="stateMachineLoader">Loader providing persisted data.</param>
        public void Load(IStateMachineLoader<TState, TEvent> stateMachineLoader)
        {
            Guard.AgainstNullArgument(nameof(stateMachineLoader), stateMachineLoader);

            this.CheckThatNotAlreadyInitialized();

            var loadedCurrentState = stateMachineLoader.LoadCurrentState();
            var historyStates = stateMachineLoader.LoadHistoryStates();
            var events = stateMachineLoader.LoadEvents();

            SetCurrentState();
            SetHistoryStates();
            SetEvents();
            NotifyExtensions();

            void SetCurrentState()
            {
                this.stateContainer.CurrentStateId = loadedCurrentState;
            }

            void SetEvents()
            {
                this.stateContainer.Events = new LinkedList<EventInformation<TEvent>>(events);
            }

            void SetHistoryStates()
            {
                foreach (var historyState in historyStates)
                {
                    var superState = this.stateDefinitions[historyState.Key];
                    var lastActiveState = historyState.Value;

                    var lastActiveStateIsNotASubStateOfSuperState = superState
                                                                        .SubStates
                                                                        .Select(x => x.Id)
                                                                        .Contains(lastActiveState)
                                                                    == false;
                    if (lastActiveStateIsNotASubStateOfSuperState)
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
                        loadedCurrentState,
                        historyStates,
                        events));
            }
        }

        /// <summary>
        /// Starts the state machine. Events will be processed.
        /// If the state machine is not started then the events will be queued until the state machine is started.
        /// Already queued events are processed.
        /// </summary>
        public void Start()
        {
            if (this.IsRunning)
            {
                return;
            }

            this.stopToken = new CancellationTokenSource();
            this.worker = Task.Factory.StartNew(
                () => this.ProcessEventQueue(this.stopToken.Token),
                this.stopToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            this.stateContainer.ForEach(extension => extension.StartedStateMachine());
        }

        /// <summary>
        /// Stops the state machine. Events will be queued until the state machine is started.
        /// </summary>
        public void Stop()
        {
            if (!this.IsRunning || this.stopToken.IsCancellationRequested)
            {
                return;
            }

            lock (this.stateContainer.Events)
            {
                this.stopToken.Cancel();
                Monitor.Pulse(this.stateContainer.Events); // wake up task to get a chance to stop
            }

            try
            {
                this.worker.Wait();
            }
            catch (AggregateException)
            {
                // in case the task was stopped before it could actually start, it will be canceled.
                if (this.worker.IsFaulted)
                {
                    throw;
                }
            }

            this.worker = null;

            this.stateContainer.ForEach(extension => extension.StoppedStateMachine());
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void AddExtension(IExtension<TState, TEvent> extension)
        {
            var extensionCompose = new InternalExtension<TState, TEvent>(extension, this.stateContainer);
            this.stateContainer.Extensions.Add(extensionCompose);
        }

        /// <summary>
        /// Clears all extensions.
        /// </summary>
        public void ClearExtensions()
        {
            this.stateContainer.Extensions.Clear();
        }

        /// <summary>
        /// Creates a state machine report with the specified generator.
        /// </summary>
        /// <param name="reportGenerator">The report generator.</param>
        public void Report(IStateMachineReport<TState, TEvent> reportGenerator)
        {
            reportGenerator.Report(this.ToString(), this.stateDefinitions.Values, this.initialState);
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

        private void ProcessEventQueue(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                this.InitializeStateMachineIfInitializationIsPending();

                EventInformation<TEvent> eventInformation;
                lock (this.stateContainer.Events)
                {
                    if (this.stateContainer.Events.Count > 0)
                    {
                        eventInformation = this.stateContainer.Events.First.Value;
                        this.stateContainer.Events.RemoveFirst();
                    }
                    else
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse because it is multi-threaded and can change in the mean time
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Monitor.Wait(this.stateContainer.Events);
                        }

                        continue;
                    }
                }

                this.stateMachine.Fire(eventInformation.EventId, eventInformation.EventArgument, this.stateContainer, this.stateDefinitions);
            }
        }

        private void InitializeStateMachineIfInitializationIsPending()
        {
            if (this.stateContainer.CurrentStateId.IsInitialized)
            {
                return;
            }

            this.stateMachine.EnterInitialState(this.stateContainer, this.stateDefinitions, this.initialState);
        }

        private void CheckThatNotAlreadyInitialized()
        {
            if (this.stateContainer.CurrentStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }
        }
    }
}