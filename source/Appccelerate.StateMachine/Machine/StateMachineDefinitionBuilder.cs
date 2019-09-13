//-------------------------------------------------------------------------------
// <copyright file="StateMachineDefinitionBuilder.cs" company="Appccelerate">
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
    using Syntax;

    public class StateMachineDefinitionBuilder<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StandardFactory<TState, TEvent> factory = new StandardFactory<TState, TEvent>();
        private readonly ImplicitAddIfNotAvailableStateDefinitionDictionary<TState, TEvent> stateDefinitionDictionary = new ImplicitAddIfNotAvailableStateDefinitionDictionary<TState, TEvent>();
        private readonly Dictionary<TState, TState> initiallyLastActiveStates = new Dictionary<TState, TState>();
        private TState initialState;
        private bool isInitialStateConfigured;

        public IEntryActionSyntax<TState, TEvent> In(TState state)
        {
            return new StateBuilder<TState, TEvent>(state, this.stateDefinitionDictionary, this.factory);
        }

        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return new HierarchyBuilder<TState, TEvent>(superStateId, this.stateDefinitionDictionary, this.initiallyLastActiveStates);
        }

        public StateMachineDefinitionBuilder<TState, TEvent> WithInitialState(TState initialStateToUse)
        {
            this.initialState = initialStateToUse;
            this.isInitialStateConfigured = true;
            return this;
        }

        public StateMachineDefinition<TState, TEvent> Build()
        {
            if (!this.isInitialStateConfigured)
            {
                throw new InvalidOperationException(ExceptionMessages.InitialStateNotConfigured);
            }

            var stateDefinitions = new StateDefinitionDictionary<TState, TEvent>(this.stateDefinitionDictionary.ReadOnlyDictionary);
            return new StateMachineDefinition<TState, TEvent>(stateDefinitions, this.initiallyLastActiveStates, this.initialState);
        }
    }
}