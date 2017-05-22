//-------------------------------------------------------------------------------
// <copyright file="TransitionCompletedEventArgs.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Events
{
    using System;

    /// <summary>
    /// Provides information about a completed transition.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class TransitionCompletedEventArgs<TState, TEvent>
        : TransitionEventArgs<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// The new state the state machine is in after the transition.
        /// </summary>
        private readonly TState newStateId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionCompletedEventArgs&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="newStateId">The new state id.</param>
        /// <param name="context">The context.</param>
        public TransitionCompletedEventArgs(TState newStateId, ITransitionContext<TState, TEvent> context) : base(context)
        {
            this.newStateId = newStateId;
        }

        /// <summary>
        /// Gets the new state id the state machine is in after the transition.
        /// </summary>
        /// <value>The new state id the state machine is in after the transition.</value>
        public TState NewStateId
        {
            get { return this.newStateId; }
        }
    }
}