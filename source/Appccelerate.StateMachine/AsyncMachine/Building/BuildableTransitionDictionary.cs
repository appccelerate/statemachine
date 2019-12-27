//-------------------------------------------------------------------------------
// <copyright file="BuildableTransitionDictionary.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.Building
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class BuildableTransitionDictionary<TState, TEvent>
        : IEnumerable<BuildableTransitionDefinition<TState, TEvent>>
        where TState : notnull
        where TEvent : notnull
    {
        private readonly List<BuildableTransitionDefinition<TState, TEvent>> transitions;
        private readonly BuildableStateDefinition<TState, TEvent> state;

        public BuildableTransitionDictionary(
            BuildableStateDefinition<TState, TEvent> state)
        {
            this.state = state;
            this.transitions = new List<BuildableTransitionDefinition<TState, TEvent>>();
        }

        public void Add(
            BuildableTransitionDefinition<TState, TEvent> transitionDefinition)
        {
            Guard.AgainstNullArgument("transition", transitionDefinition);

            this.CheckTransitionDoesNotYetExist(transitionDefinition);

            transitionDefinition.Source = this.state;

            this.transitions.Add(transitionDefinition);
        }

        private void CheckTransitionDoesNotYetExist(
            BuildableTransitionDefinition<TState, TEvent> transitionDefinition)
        {
            if (transitionDefinition.Source != null)
            {
                throw new InvalidOperationException(
                    BuildingExceptionMessages.TransitionDoesAlreadyExist(
                        transitionDefinition,
                        this.state));
            }
        }

        public IEnumerator<BuildableTransitionDefinition<TState, TEvent>> GetEnumerator()
        {
            return this.transitions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}