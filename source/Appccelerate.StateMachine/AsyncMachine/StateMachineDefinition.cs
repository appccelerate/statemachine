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
        private readonly IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates;

        public StateMachineDefinition(
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions,
            IReadOnlyDictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates)
        {
            this.stateDefinitions = stateDefinitions;
            this.initiallyLastActiveStates = initiallyLastActiveStates;
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

            var transitionLogic = new TransitionLogic<TState, TEvent>(stateContainer, stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, stateContainer, stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            var standardFactory = new StandardFactoryNew<TState, TEvent>();
            var stateMachine = new StateMachineNew<TState, TEvent>(standardFactory, stateLogic);

            return new AsyncPassiveStateMachine<TState, TEvent>(stateMachine, stateContainer, this.stateDefinitions);
        }
    }
}