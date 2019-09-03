//-------------------------------------------------------------------------------
// <copyright file="StateDictionary.cs" company="Appccelerate">
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
    using System.Collections.Generic;

    using States;

    public class StateDictionary<TState, TEvent> : IStateDictionary<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly Dictionary<TState, StateDefinition<TState, TEvent>> dictionary = new Dictionary<TState, StateDefinition<TState, TEvent>>();

        public StateDefinition<TState, TEvent> this[TState stateId]
        {
            get
            {
                if (!this.dictionary.ContainsKey(stateId))
                {
                    this.dictionary.Add(stateId, new StateDefinition<TState, TEvent>(stateId));
                }

                return this.dictionary[stateId];
            }
        }

        public IReadOnlyDictionary<TState, StateDefinition<TState, TEvent>> ReadOnlyDictionary => this.dictionary;
    }
}