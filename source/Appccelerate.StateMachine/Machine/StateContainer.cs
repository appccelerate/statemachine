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

namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;
    using Infrastructure;

    public class StateContainer<TState, TEvent> :
        IExtensionHost<TState, TEvent>,
        IStateMachineInformation<TState, TEvent>,
        ILastActiveStateModifier<TState>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly Dictionary<TState, TState> lastActiveStates = new Dictionary<TState, TState>();

        public StateContainer()
            : this(default(string))
        {
        }

        public StateContainer(string name)
        {
            this.Name = name;
            this.CurrentStateId = Initializable<TState>.UnInitialized();
        }

        public string Name { get; }

        public List<IExtensionInternal<TState, TEvent>> Extensions { get; } = new List<IExtensionInternal<TState, TEvent>>();

        public IInitializable<TState> CurrentStateId { get; set; }

        public IReadOnlyDictionary<TState, TState> LastActiveStates => this.lastActiveStates;

        public LinkedList<EventInformation<TEvent>> Events { get; set; } = new LinkedList<EventInformation<TEvent>>();

        public IReadOnlyCollection<EventInformation<TEvent>> SaveableEvents => new List<EventInformation<TEvent>>(this.Events);

        public void ForEach(Action<IExtensionInternal<TState, TEvent>> action)
        {
            this.Extensions.ForEach(action);
        }

        public Optional<TState> GetLastActiveStateFor(TState state)
        {
            return
                this.lastActiveStates.TryGetValue(state, out var lastActiveState)
                    ? Optional<TState>.Just(lastActiveState)
                    : Optional<TState>.Nothing();
        }

        public void SetLastActiveStateFor(TState state, TState newLastActiveState)
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
