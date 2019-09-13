//-------------------------------------------------------------------------------
// <copyright file="StateMachineInitializer.cs" company="Appccelerate">
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

    /// <summary>
    /// Responsible for entering the initial state of the state machine.
    /// All states up in the hierarchy are entered, too.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachineInitializer<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDefinition<TState, TEvent> initialState;
        private readonly ITransitionContext<TState, TEvent> context;

        public StateMachineInitializer(
            IStateDefinition<TState, TEvent> initialState,
            ITransitionContext<TState, TEvent> context)
        {
            this.initialState = initialState;
            this.context = context;
        }

        public TState EnterInitialState(
            IStateLogic<TState, TEvent> stateLogic,
            ILastActiveStateModifier<TState> lastActiveStateModifier,
            IStateDefinitionDictionary<TState, TEvent> stateDefinitions)
        {
            var stack = this.TraverseUpTheStateHierarchy();
            this.TraverseDownTheStateHierarchyAndEnterStates(stateLogic, stack);

            return stateLogic.EnterByHistory(this.initialState, this.context, lastActiveStateModifier, stateDefinitions);
        }

        /// <summary>
        /// Traverses up the state hierarchy and build the stack of states.
        /// </summary>
        /// <returns>The stack containing all states up the state hierarchy.</returns>
        private Stack<IStateDefinition<TState, TEvent>> TraverseUpTheStateHierarchy()
        {
            var stack = new Stack<IStateDefinition<TState, TEvent>>();

            var state = this.initialState;
            while (state != null)
            {
                stack.Push(state);
                state = state.SuperState;
            }

            return stack;
        }

        private void TraverseDownTheStateHierarchyAndEnterStates(
            IStateLogic<TState, TEvent> stateLogic,
            Stack<IStateDefinition<TState, TEvent>> stack)
        {
            while (stack.Count > 0)
            {
                var state = stack.Pop();
                stateLogic.Entry(state, this.context);
            }
        }
    }
}