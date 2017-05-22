//-------------------------------------------------------------------------------
// <copyright file="TransitionEventArgs.cs" company="Appccelerate">
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
    using System.Globalization;

    /// <summary>
    /// Event arguments providing a transition context.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class TransitionEventArgs<TState, TEvent>
        : ContextEventArgs<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionEventArgs&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="context">The event context.</param>
        public TransitionEventArgs(ITransitionContext<TState, TEvent> context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets the id of the source state of the transition.
        /// </summary>
        /// <value>The id of the source state of the transition.</value>
        public TState StateId
        {
            get { return this.Context.State.Id; }
        }

        /// <summary>
        /// Gets the event id.
        /// </summary>
        /// <value>The event id.</value>
        public TEvent EventId
        {
            get { return this.Context.EventId.Value; }
        }

        /// <summary>
        /// Gets the event argument.
        /// </summary>
        /// <value>The event argument.</value>
        public object EventArgument
        {
            get { return this.Context.EventArgument; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Transition from state {0} on event {1}.", this.StateId, this.EventId);
        }
    }
}