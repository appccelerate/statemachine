//-------------------------------------------------------------------------------
// <copyright file="IStateLogic.cs" company="Appccelerate">
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
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.Machine.States
{
    using System;

    public interface IStateLogic<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Fires the specified event id on this state.
        /// </summary>
        /// <param name="stateDefinition">The definition of the state onto which the event should be fired.</param>
        /// <param name="context">The event context.</param>
        /// <param name="lastActiveStateModifier">The last active state modifier.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        /// <returns>The result of the transition.</returns>
        ITransitionResult<TState> Fire(IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, ILastActiveStateModifier<TState> lastActiveStateModifier, IStateDefinitionDictionary<TState, TEvent> stateDefinitions);

        void Entry(IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context);

        void Exit(IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, ILastActiveStateModifier<TState> lastActiveStateModifier);

        TState EnterByHistory(IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, ILastActiveStateModifier<TState> lastActiveStateModifier, IStateDefinitionDictionary<TState, TEvent> stateDefinitions);
    }
}