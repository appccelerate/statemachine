//-------------------------------------------------------------------------------
// <copyright file="ITransition.cs" company="Appccelerate">
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

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.GuardHolders;

    /// <summary>
    /// Represents a transition in the state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface ITransition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Gets or sets the source state of the transition.
        /// </summary>
        /// <value>The source.</value>
        IState<TState, TEvent> Source { get; set; }

        /// <summary>
        /// Gets or sets the target state of the transition.
        /// </summary>
        /// <value>The target.</value>
        IState<TState, TEvent> Target { get; set; }

        /// <summary>
        /// Gets the actions of this transition.
        /// </summary>
        /// <value>The actions.</value>
        ICollection<IActionHolder> Actions { get; }

        /// <summary>
        /// Gets or sets the guard of this transition.
        /// </summary>
        /// <value>The guard.</value>
        IGuardHolder Guard { get; set; }

        /// <summary>
        /// Fires the transition.
        /// </summary>
        /// <param name="context">The event context.</param>
        /// <returns>The result of the transition.</returns>
        ITransitionResult<TState, TEvent> Fire(ITransitionContext<TState, TEvent> context);
    }
}