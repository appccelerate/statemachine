//-------------------------------------------------------------------------------
// <copyright file="StateContainer.cs" company="Appccelerate">
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
    using System.Threading.Tasks;
    using Infrastructure;
    using States;

    public class StateContainer<TState, TEvent> :
        IExtensionHost<TState, TEvent>,
        IStateMachineInformation<TState, TEvent>,
        ILastActiveStateModifier<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly Dictionary<TState, IStateDefinition<TState, TEvent>> lastActiveStates = new Dictionary<TState, IStateDefinition<TState, TEvent>>();

        public StateContainer()
            : this(default(string))
        {
        }

        public StateContainer(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public List<IExtension<TState, TEvent>> Extensions { get; } = new List<IExtension<TState, TEvent>>();

        public Initializable<TState> InitialStateId { get; } = new Initializable<TState>();

        public IStateDefinition<TState, TEvent> CurrentState { get; set; }

        public TState CurrentStateId => this.CurrentState.Id;

        public IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> LastActiveStates => this.lastActiveStates;

        public async Task ForEach(Func<IExtension<TState, TEvent>, Task> action)
        {
            foreach (var extension in this.Extensions)
            {
                await action(extension)
                    .ConfigureAwait(false);
            }
        }

        public IStateDefinition<TState, TEvent> GetLastActiveStateOrNullFor(TState state)
        {
            this.lastActiveStates.TryGetValue(state, out var lastActiveState);
            return lastActiveState;
        }

        public void SetLastActiveStateFor(TState state, IStateDefinition<TState, TEvent> newLastActiveState)
        {
            if (this.lastActiveStates.ContainsKey(state))
            {
                this.lastActiveStates[state] = newLastActiveState;
            }
            else
            {
                this.lastActiveStates.Add(state, newLastActiveState);
            }
        }
    }
}
