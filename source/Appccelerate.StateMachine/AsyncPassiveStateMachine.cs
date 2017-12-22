// <copyright file="AsyncPassiveStateMachine.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Appccelerate.StateMachine.AsyncMachine;
    using Appccelerate.StateMachine.AsyncMachine.Events;
    using Appccelerate.StateMachine.AsyncSyntax;
    using Appccelerate.StateMachine.Persistence;

    public class AsyncPassiveStateMachine<TState, TEvent> : IAsyncStateMachine<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StateMachine<TState, TEvent> stateMachine;

        private readonly ConcurrentQueue<EventInformation<TEvent>> events;
        private readonly ConcurrentStack<EventInformation<TEvent>> priorityEvents;

        private bool initialized;
        private bool executing;
        private bool pendingInitialization;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPassiveStateMachine&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        public AsyncPassiveStateMachine()
            : this(default(string))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPassiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of the state machine. Used in log messages.</param>
        public AsyncPassiveStateMachine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPassiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of the state machine. Used in log messages.</param>
        /// <param name="factory">The factory.</param>
        public AsyncPassiveStateMachine(string name, IFactory<TState, TEvent> factory)
        {
            this.stateMachine = new StateMachine<TState, TEvent>(
                name ?? this.GetType().FullNameToString(),
                factory);
            this.events = new ConcurrentQueue<EventInformation<TEvent>>();
            this.priorityEvents = new ConcurrentStack<EventInformation<TEvent>>();
        }

        /// <summary>
        /// Occurs when no transition could be executed.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined
        {
            add { this.stateMachine.TransitionDeclined += value; }
            remove { this.stateMachine.TransitionDeclined -= value; }
        }

        /// <summary>
        /// Occurs when an exception was thrown inside a transition of the state machine.
        /// </summary>
        public event EventHandler<TransitionExceptionEventArgs<TState, TEvent>> TransitionExceptionThrown
        {
            add { this.stateMachine.TransitionExceptionThrown += value; }
            remove { this.stateMachine.TransitionExceptionThrown -= value; }
        }

        /// <summary>
        /// Occurs when a transition begins.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionBegin
        {
            add { this.stateMachine.TransitionBegin += value; }
            remove { this.stateMachine.TransitionBegin -= value; }
        }

        /// <summary>
        /// Occurs when a transition completed.
        /// </summary>
        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted
        {
            add { this.stateMachine.TransitionCompleted += value; }
            remove { this.stateMachine.TransitionCompleted -= value; }
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
        /// Define the behavior of a state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Syntax to build state behavior.</returns>
        public IEntryActionSyntax<TState, TEvent> In(TState state)
        {
            return this.stateMachine.In(state);
        }

        /// <summary>
        /// Defines the hierarchy on.
        /// </summary>
        /// <param name="superStateId">The super state id.</param>
        /// /// <returns>Syntax to build a state hierarchy.</returns>
        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return this.stateMachine.DefineHierarchyOn(superStateId);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Fire(TEvent eventId)
        {
            await this.Fire(eventId, Missing.Value).ConfigureAwait(false);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Fire(TEvent eventId, object eventArgument)
        {
            this.events.Enqueue(new EventInformation<TEvent>(eventId, eventArgument));

            await this.stateMachine
                .ForEach(extension => extension.EventQueued(this.stateMachine, eventId, eventArgument))
                .ConfigureAwait(false);

            await this.Execute().ConfigureAwait(false);
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FirePriority(TEvent eventId)
        {
            await this.FirePriority(eventId, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Fires the specified priority event. The event will be handled before any already queued event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FirePriority(TEvent eventId, object eventArgument)
        {
            this.priorityEvents.Push(new EventInformation<TEvent>(eventId, eventArgument));

            await this.stateMachine
                .ForEach(extension => extension.EventQueuedWithPriority(this.stateMachine, eventId, eventArgument))
                .ConfigureAwait(false);

            await this.Execute().ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the state machine to the specified initial state.
        /// </summary>
        /// <param name="initialState">The state to which the state machine is initialized.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Initialize(TState initialState)
        {
            this.CheckThatNotAlreadyInitialized();

            this.initialized = true;
            this.pendingInitialization = true;

            await this.stateMachine.Initialize(initialState)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Starts the state machine. Events will be processed.
        /// If the state machine is not started then the events will be queued until the state machine is started.
        /// Already queued events are processed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Start()
        {
            this.CheckThatStateMachineIsInitialized();

            this.IsRunning = true;

            await this.stateMachine.ForEach(extension => extension.StartedStateMachine(this.stateMachine))
                .ConfigureAwait(false);

            await this.Execute().ConfigureAwait(false);
        }

        /// <summary>
        /// Clears all extensions.
        /// </summary>
        public void ClearExtensions()
        {
            this.stateMachine.ClearExtensions();
        }

        /// <summary>
        /// Creates a state machine report with the specified generator.
        /// </summary>
        /// <param name="reportGenerator">The report generator.</param>
        public void Report(IStateMachineReport<TState, TEvent> reportGenerator)
        {
            this.stateMachine.Report(reportGenerator);
        }

        /// <summary>
        /// Stops the state machine. Events will be queued until the state machine is started.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Stop()
        {
            this.IsRunning = false;

            await this.stateMachine.ForEach(extension => extension.StoppedStateMachine(this.stateMachine))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void AddExtension(IExtension<TState, TEvent> extension)
        {
            this.stateMachine.AddExtension(extension);
        }

        /// <summary>
        /// Saves the current state and history states to a persisted state. Can be restored using <see cref="Load"/>.
        /// </summary>
        /// <param name="stateMachineSaver">Data to be persisted is passed to the saver.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Save(IAsyncStateMachineSaver<TState> stateMachineSaver)
        {
            Guard.AgainstNullArgument("stateMachineSaver", stateMachineSaver);

            await this.stateMachine.Save(stateMachineSaver).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the current state and history states from a persisted state (<see cref="Save"/>).
        /// The loader should return exactly the data that was passed to the saver.
        /// </summary>
        /// <param name="stateMachineLoader">Loader providing persisted data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Load(IAsyncStateMachineLoader<TState> stateMachineLoader)
        {
            Guard.AgainstNullArgument("stateMachineLoader", stateMachineLoader);

            this.CheckThatNotAlreadyInitialized();

            this.initialized = await this.stateMachine.Load(stateMachineLoader).ConfigureAwait(false);
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

        private async Task Execute()
        {
            if (this.executing || !this.IsRunning)
            {
                return;
            }

            this.executing = true;
            try
            {
                await this.ProcessQueuedEvents().ConfigureAwait(false);
            }
            finally
            {
                this.executing = false;
            }
        }

        private async Task ProcessQueuedEvents()
        {
            await this.InitializeStateMachineIfInitializationIsPending().ConfigureAwait(false);

            while (this.priorityEvents.TryPop(out var eventInformation))
            {
                await this.stateMachine.Fire(
                        eventInformation.EventId,
                        eventInformation.EventArgument)
                    .ConfigureAwait(false);
            }

            while (this.events.TryDequeue(out var eventInformation))
            {
                await this.stateMachine.Fire(
                        eventInformation.EventId,
                        eventInformation.EventArgument)
                    .ConfigureAwait(false);
            }
        }

        private async Task InitializeStateMachineIfInitializationIsPending()
        {
            if (!this.pendingInitialization)
            {
                return;
            }

            await this.stateMachine.EnterInitialState().ConfigureAwait(false);

            this.pendingInitialization = false;
        }
    }
}