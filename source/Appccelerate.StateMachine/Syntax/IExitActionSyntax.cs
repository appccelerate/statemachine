//-------------------------------------------------------------------------------
// <copyright file="IExitActionSyntax.cs" company="Appccelerate">
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
    /// Defines the exit action syntax.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IExitActionSyntax<TState, TEvent> : IEventSyntax<TState, TEvent>
    {
        /// <summary>
        /// Defines an exit action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>Event syntax.</returns>
        IExitActionSyntax<TState, TEvent> ExecuteOnExit(Action action);

        /// <summary>
        /// Defines an exit action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>Event syntax.</returns>
        /// <typeparam name="T">Type of the event argument passed to the action.</typeparam>
        IExitActionSyntax<TState, TEvent> ExecuteOnExit<T>(Action<T> action);

        /// <summary>
        /// Defines an exit action.
        /// </summary>
        /// <typeparam name="T">Type of the parameter of the exit action method.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="parameter">The parameter that will be passed to the exit action.</param>
        /// <returns>Exit action syntax.</returns>
        IExitActionSyntax<TState, TEvent> ExecuteOnExitParametrized<T>(Action<T> action, T parameter);
    }
}