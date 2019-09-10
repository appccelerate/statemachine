//-------------------------------------------------------------------------------
// <copyright file="StateDefinitionDictionary.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine
{
    using System;
    using System.Collections.Generic;
    using States;

    public class StateDefinitionDictionary<TState, TEvent> : IStateDefinitionDictionary<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> stateDefinitions;

        public StateDefinitionDictionary(IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> stateDefinitions)
        {
            this.stateDefinitions = stateDefinitions;
        }

        public IStateDefinition<TState, TEvent> this[TState key]
        {
            get
            {
                if (this.stateDefinitions.TryGetValue(key, out var stateDefinition))
                {
                    return stateDefinition;
                }

                throw new InvalidOperationException(
                    ExceptionMessages.CannotFindStateDefinition(key));
            }
        }

        public IEnumerable<IStateDefinition<TState, TEvent>> Values => this.stateDefinitions.Values;
    }
}