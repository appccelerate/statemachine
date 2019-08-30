//-------------------------------------------------------------------------------
// <copyright file="StateMachine.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using Appccelerate.StateMachine.Machine.Events;
    using Appccelerate.StateMachine.Syntax;

    /// <summary>
    /// Base implementation of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachine<TState, TEvent> :
        INotifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IFactory<TState, TEvent> factory;
        private readonly IStateLogic<TState, TEvent> stateLogic;
        private readonly IStateDictionaryNew<TState, TEvent> stateDefinitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        public StateMachine()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState,TEvent}"/> class.
        /// </summary>
        /// <param name="name">The name of this state machine used in log messages.</param>
        /// <param name="factory">The factory used to create internal instances.</param>
        public StateMachine(IFactory<TState, TEvent> factory, IStateLogic<TState, TEvent> stateLogic, IStateDictionaryNew<TState, TEvent> stateDefinitions)
        {
            this.factory = factory;
            this.stateLogic = stateLogic;
            this.stateDefinitions = stateDefinitions;
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

        private static void SwitchStateTo(TState newState, StateContainer<TState, TEvent> stateContainer, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            var oldState = stateContainer.CurrentState;

            stateContainer.CurrentState = newState;

            stateContainer.Extensions.ForEach(extension =>
                extension.SwitchedState(stateMachineInformation, oldState, newState));
        }

        /// <summary>
        /// Define the behavior of a state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Syntax to build state behavior.</returns>
        public IEntryActionSyntax<TState, TEvent> In(TState state)
        {
            return null;
        }

        /// <summary>
        /// Defines the hierarchy on.
        /// </summary>
        /// <param name="superStateId">The super state id.</param>
        /// <returns>Syntax to build a state hierarchy.</returns>
        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return null;
        }

        /// <summary>
        /// Initializes the state machine by setting the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state of the state machine.</param>
        public void Initialize(TState initialState, StateContainer<TState, TEvent> stateContainer, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            stateContainer.Extensions.ForEach(extension => extension.InitializingStateMachine(stateMachineInformation, ref initialState));

            Initialize(initialState, stateContainer);

            stateContainer.Extensions.ForEach(extension => extension.InitializedStateMachine(stateMachineInformation, initialState));
        }

        /// <summary>
        /// Enters the initial state that was previously set with <see cref="Initialize(TState)"/>.
        /// </summary>
        public void EnterInitialState(StateContainer<TState, TEvent> stateContainer, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            CheckThatStateMachineIsInitialized(stateContainer);

            stateContainer.Extensions.ForEach(extension => extension.EnteringInitialState(stateMachineInformation, stateContainer.InitialStateId.Value));

            var context = this.factory.CreateTransitionContext(null, new Missable<TEvent>(), Missing.Value, this);
            this.EnterInitialState(context, stateContainer, stateMachineInformation);

            stateContainer.Extensions.ForEach(extension => extension.EnteredInitialState(stateMachineInformation, stateContainer.InitialStateId.Value, context));
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        public void Fire(TEvent eventId, StateContainer<TState, TEvent> stateContainer, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            this.Fire(eventId, Missing.Value, stateContainer, stateMachineInformation);
        }

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="eventArgument">The event argument.</param>
        public void Fire(TEvent eventId, object eventArgument, StateContainer<TState, TEvent> stateContainer, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            CheckThatStateMachineIsInitialized(stateContainer);
            CheckThatStateMachineHasEnteredInitialState(stateContainer);

            stateContainer.Extensions.ForEach(extension => extension.FiringEvent(stateMachineInformation, ref eventId, ref eventArgument));

            var stateDefinition = this.stateDefinitions[stateContainer.CurrentState];
            ITransitionContext<TState, TEvent> context = this.factory.CreateTransitionContext(stateDefinition, new Missable<TEvent>(eventId), eventArgument, this);
            var result = this.stateLogic.Fire(stateDefinition, context, stateContainer);

            if (!result.Fired)
            {
                this.OnTransitionDeclined(context);
                return;
            }

            SwitchStateTo(result.NewState, stateContainer, stateMachineInformation);

            stateContainer.Extensions.ForEach(extension => extension.FiredEvent(stateMachineInformation, context));

            this.OnTransitionCompleted(context, stateMachineInformation);
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
        
        // Todo: wtjerry
        //        /// <summary>
        //        /// Creates a report with the specified generator.
        //        /// </summary>
        //        /// <param name="reportGenerator">The report generator.</param>
        //        public void Report(IStateMachineReport<TState, TEvent> reportGenerator)
        //        {
        //            Guard.AgainstNullArgument("reportGenerator", reportGenerator);
        //
        //            reportGenerator.Report(this.ToString(), this.stateContainer.States.GetStates(), this.stateContainer.InitialStateId);
        //        }
        //
        //        public void Save(IStateMachineSaver<TState> stateMachineSaver)
        //        {
        //            Guard.AgainstNullArgument("stateMachineSaver", stateMachineSaver);
        //
        //            stateMachineSaver.SaveCurrentState(this.stateContainer.CurrentState != null ?
        //                new Initializable<TState> { Value = this.stateContainer.CurrentState.Id } :
        //                new Initializable<TState>());
        //
        //            IEnumerable<IState<TState, TEvent>> superStatesWithLastActiveState = this.stateContainer.States.GetStates()
        //                .Where(s => s.SubStates.Any())
        //                .Where(s => s.LastActiveState != null)
        //                .ToList();
        //
        //            var historyStates = superStatesWithLastActiveState.ToDictionary(
        //                s => s.Id,
        //                s => s.LastActiveState.Id);
        //
        //            stateMachineSaver.SaveHistoryStates(historyStates);
        //        }
        //
        //        public bool Load(IStateMachineLoader<TState> stateMachineLoader)
        //        {
        //            Guard.AgainstNullArgument(nameof(stateMachineLoader), stateMachineLoader);
        //            this.CheckThatStateMachineIsNotAlreadyInitialized();
        //
        //            Initializable<TState> loadedCurrentState = stateMachineLoader.LoadCurrentState();
        //            IDictionary<TState, TState> historyStates = stateMachineLoader.LoadHistoryStates();
        //
        //            var initialized = SetCurrentState();
        //            LoadHistoryStates();
        //            NotifyExtensions();
        //
        //            return initialized;
        //
        //            bool SetCurrentState()
        //            {
        //                if (loadedCurrentState.IsInitialized)
        //                {
        //                    this.stateContainer.CurrentState = this.stateContainer.States[loadedCurrentState.Value];
        //                    return true;
        //                }
        //
        //                this.stateContainer.CurrentState = null;
        //                return false;
        //            }
        //
        //            void LoadHistoryStates()
        //            {
        //                foreach (KeyValuePair<TState, TState> historyState in historyStates)
        //                {
        //                    IState<TState, TEvent> superState = this.stateContainer.States[historyState.Key];
        //                    IState<TState, TEvent> lastActiveState = this.stateContainer.States[historyState.Value];
        //
        //                    if (!superState.SubStates.Contains(lastActiveState))
        //                    {
        //                        throw new InvalidOperationException(ExceptionMessages.CannotSetALastActiveStateThatIsNotASubState);
        //                    }
        //
        //                    superState.LastActiveState = lastActiveState;
        //                }
        //            }
        //
        //            void NotifyExtensions()
        //            {
        //                this.stateContainer.Extensions.ForEach(
        //                    extension => extension.Loaded(
        //                        this,
        //                        loadedCurrentState,
        //                        historyStates));
        //            }
        //        }

        // ReSharper disable once UnusedParameter.Local
        private static void RethrowExceptionIfNoHandlerRegistered<T>(Exception exception, EventHandler<T> exceptionHandler)
            where T : EventArgs
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
        private void OnTransitionCompleted(ITransitionContext<TState, TEvent> transitionContext, IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            this.RaiseEvent(
                this.TransitionCompleted,
                new TransitionCompletedEventArgs<TState, TEvent>(
                    stateMachineInformation.CurrentStateId,
                    transitionContext),
                transitionContext,
                true);
        }

        /// <summary>
        /// Initializes the state machine by setting the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        private static void Initialize(TState initialState, StateContainer<TState, TEvent> stateContainer)
        {
            if (stateContainer.InitialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
            }

            stateContainer.InitialStateId.Value = initialState;
        }

        private void EnterInitialState(
            ITransitionContext<TState, TEvent> context,
            StateContainer<TState, TEvent> stateContainer,
            IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            var initialState = this.stateDefinitions[stateContainer.InitialStateId.Value];
            var initializer = this.factory.CreateStateMachineInitializer(initialState, context);
            var newState = initializer.EnterInitialState(this.stateLogic, stateContainer);
            SwitchStateTo(newState, stateContainer, stateMachineInformation);
        }

        private void RaiseEvent<T>(EventHandler<T> eventHandler, T arguments, ITransitionContext<TState, TEvent> context, bool raiseEventOnException)
            where T : EventArgs
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

        private static void CheckThatStateMachineIsInitialized(StateContainer<TState, TEvent> stateContainer)
        {
            if (stateContainer.CurrentState == null && !stateContainer.InitialStateId.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineNotInitialized);
            }
        }
//
//        private void CheckThatStateMachineIsNotAlreadyInitialized()
//        {
//            if (this.stateContainer.CurrentState != null || this.stateContainer.InitialStateId.IsInitialized)
//            {
//                throw new InvalidOperationException(ExceptionMessages.StateMachineIsAlreadyInitialized);
//            }
//        }

        private static void CheckThatStateMachineHasEnteredInitialState(StateContainer<TState, TEvent> stateContainer)
        {
            if (stateContainer.CurrentState == null)
            {
                throw new InvalidOperationException(ExceptionMessages.StateMachineHasNotYetEnteredInitialState);
            }
        }
    }
}