//-------------------------------------------------------------------------------
// <copyright file="StateMachineDefinition.cs" company="Appccelerate">
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
    using Transitions;

    public class StateMachineDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDefinitionDictionary<TState, TEvent> stateDefinitions;
        private readonly IReadOnlyDictionary<TState, TState> initiallyLastActiveStates;
        private readonly TState initialState;

        public StateMachineDefinition(
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions,
            IReadOnlyDictionary<TState, TState> initiallyLastActiveStates,
            TState initialState)
        {
            this.stateDefinitions = stateDefinitions;
            this.initiallyLastActiveStates = initiallyLastActiveStates;
            this.initialState = initialState;
        }

        public AsyncPassiveStateMachine<TState, TEvent> CreatePassiveStateMachine()
        {
            var name = typeof(AsyncPassiveStateMachine<TState, TEvent>).FullNameToString();
            return this.CreatePassiveStateMachine(name);
        }

        public AsyncPassiveStateMachine<TState, TEvent> CreatePassiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            foreach (var stateIdAndLastActiveState in this.initiallyLastActiveStates)
            {
                stateContainer.SetLastActiveStateFor(stateIdAndLastActiveState.Key, stateIdAndLastActiveState.Value);
            }

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var standardFactory = new StandardFactory<TState, TEvent>();
            var stateMachine = new StateMachine<TState, TEvent>(standardFactory, stateLogic);

            return new AsyncPassiveStateMachine<TState, TEvent>(stateMachine, stateContainer, this.stateDefinitions, this.initialState);
        }

        public AsyncActiveStateMachine<TState, TEvent> CreateActiveStateMachine()
        {
            var name = typeof(AsyncActiveStateMachine<TState, TEvent>).FullNameToString();
            return this.CreateActiveStateMachine(name);
        }

        public AsyncActiveStateMachine<TState, TEvent> CreateActiveStateMachine(string name)
        {
            var stateContainer = new StateContainer<TState, TEvent>(name);
            foreach (var stateIdAndLastActiveState in this.initiallyLastActiveStates)
            {
                stateContainer.SetLastActiveStateFor(stateIdAndLastActiveState.Key, stateIdAndLastActiveState.Value);
            }

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var standardFactory = new StandardFactory<TState, TEvent>();
            var stateMachine = new StateMachine<TState, TEvent>(standardFactory, stateLogic);

            return new AsyncActiveStateMachine<TState, TEvent>(stateMachine, stateContainer, this.stateDefinitions, this.initialState);
        }
    }
}