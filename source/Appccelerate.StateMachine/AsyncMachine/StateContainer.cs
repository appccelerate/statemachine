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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class StateContainer<TState, TEvent> :
        IExtensionHost<TState, TEvent>,
        IStateMachineInformation<TState, TEvent>,
        ILastActiveStateModifier<TState>
        where TState : notnull
        where TEvent : notnull
    {
        private readonly Dictionary<TState, TState> lastActiveStates = new Dictionary<TState, TState>();

        public StateContainer(string name)
        {
            this.Name = name;
            this.CurrentStateId = Option<TState>.None;
        }

        public string Name { get; }

        public List<IExtensionInternal<TState, TEvent>> Extensions { get; } = new List<IExtensionInternal<TState, TEvent>>();

        public Option<TState> CurrentStateId { get; set; }

        public IReadOnlyDictionary<TState, TState> LastActiveStates => this.lastActiveStates;

        public ConcurrentQueue<EventInformation<TEvent>> Events { get; set; } = new ConcurrentQueue<EventInformation<TEvent>>();

        public IReadOnlyCollection<EventInformation<TEvent>> SaveableEvents => new List<EventInformation<TEvent>>(this.Events);

        public ConcurrentStack<EventInformation<TEvent>> PriorityEvents { get; set; } = new ConcurrentStack<EventInformation<TEvent>>();

        public IReadOnlyCollection<EventInformation<TEvent>> SaveablePriorityEvents => new List<EventInformation<TEvent>>(this.PriorityEvents);

        public async Task ForEach(Func<IExtensionInternal<TState, TEvent>, Task> action)
        {
            foreach (var extension in this.Extensions)
            {
                await action(extension)
                    .ConfigureAwait(false);
            }
        }

        public Option<TState> GetLastActiveStateFor(TState state)
        {
            return
                this.lastActiveStates.TryGetValue(state, out var lastActiveState)
                    ? Option<TState>.Some(lastActiveState)
                    : Option<TState>.None;
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
