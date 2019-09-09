//-------------------------------------------------------------------------------
// <copyright file="SyntaxStart.cs" company="Appccelerate">
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
    using States;
    using Syntax;

    public class SyntaxStart<TState, TEvent> : ISyntaxStart<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDictionary<TState, TEvent> stateDefinitionDictionary;
        private readonly IDictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates;

        public SyntaxStart(
            IStateDictionary<TState, TEvent> stateDefinitionDictionary,
            IDictionary<TState, IStateDefinition<TState, TEvent>> initiallyLastActiveStates)
        {
            this.stateDefinitionDictionary = stateDefinitionDictionary;
            this.initiallyLastActiveStates = initiallyLastActiveStates;
        }

        public IEntryActionSyntax<TState, TEvent> In(TState stateId)
        {
            var standardFactory = new StandardFactory<TState, TEvent>();
            return new StateBuilder<TState, TEvent>(stateId, this.stateDefinitionDictionary, standardFactory);
        }

        public IHierarchySyntax<TState> DefineHierarchyOn(TState superStateId)
        {
            return new HierarchyBuilder<TState, TEvent>(superStateId, this.stateDefinitionDictionary, this.initiallyLastActiveStates);
        }
    }
}