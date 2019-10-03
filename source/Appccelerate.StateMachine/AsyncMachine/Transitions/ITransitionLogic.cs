//-------------------------------------------------------------------------------
// <copyright file="ITransitionLogic.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.Transitions
{
    using System;
    using System.Threading.Tasks;

    public interface ITransitionLogic<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Fires the transition.
        /// </summary>
        /// <param name="transitionDefinition">The definition of the transition which should happen.</param>
        /// <param name="context">The event context.</param>
        /// <param name="lastActiveStateModifier">The last active state modifier.</param>
        /// <param name="stateDefinitions">The definitions for all states of this state Machine.</param>
        /// <returns>The result of the transition.</returns>
        Task<ITransitionResult<TState>> Fire(
            ITransitionDefinition<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions);
    }
}
