//-------------------------------------------------------------------------------
// <copyright file="IIfOrOtherwiseSyntax.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Syntax
{
    using System;

    /// <summary>
    /// Defines the syntax for If or Otherwise.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IIfOrOtherwiseSyntax<TState, TEvent> : IEventSyntax<TState, TEvent>
    {
        /// <summary>
        /// Defines a transition guard. The transition is only taken if the guard is fulfilled.
        /// </summary>
        /// <typeparam name="T">The type of the guard argument.</typeparam>
        /// <param name="guard">The guard.</param>
        /// <returns>If syntax.</returns>
        IIfSyntax<TState, TEvent> If<T>(Func<T, bool> guard);

        /// <summary>
        /// Defines a transition guard. The transition is only taken if the guard is fulfilled.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <returns>If syntax.</returns>
        IIfSyntax<TState, TEvent> If(Func<bool> guard);

        /// <summary>
        /// Defines the transition that is taken when the guards of all other transitions did not match.
        /// </summary>
        /// <returns>Default syntax.</returns>
        IOtherwiseSyntax<TState, TEvent> Otherwise();

        /// <summary>
        /// Defines the transition actions.
        /// </summary>
        /// <param name="action">The actions to execute when the transition is taken.</param>
        /// <returns>Event syntax</returns>
        IIfOrOtherwiseSyntax<TState, TEvent> Execute(Action action);

        /// <summary>
        /// Defines the transition actions.
        /// </summary>
        /// <typeparam name="T">The type of the action argument.</typeparam>
        /// <param name="action">The actions to execute when the transition is taken.</param>
        /// <returns>Event syntax</returns>
        IIfOrOtherwiseSyntax<TState, TEvent> Execute<T>(Action<T> action);
    }
}