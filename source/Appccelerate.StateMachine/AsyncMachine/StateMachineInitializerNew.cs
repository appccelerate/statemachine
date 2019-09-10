//-------------------------------------------------------------------------------
// <copyright file="StateMachineInitializerNew.cs" company="Appccelerate">
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
    using States;

    /// <summary>
    /// Responsible for entering the initial state of the state machine.
    /// All states up in the hierarchy are entered, too.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachineInitializerNew<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateDefinition<TState, TEvent> initialState;

        private readonly ITransitionContextNew<TState, TEvent> context;

        public StateMachineInitializerNew(IStateDefinition<TState, TEvent> initialState, ITransitionContextNew<TState, TEvent> context)
        {
            this.initialState = initialState;
            this.context = context;
        }

        public async Task<TState> EnterInitialState(
            IStateLogic<TState, TEvent> stateLogic,
            ILastActiveStateModifier<TState, TEvent> lastActiveStateModifier)
        {
            var stack = this.TraverseUpTheStateHierarchy();
            await this.TraverseDownTheStateHierarchyAndEnterStates(stateLogic, stack)
                .ConfigureAwait(false);

            return await stateLogic.EnterByHistory(this.initialState, this.context, lastActiveStateModifier)
                .ConfigureAwait(false);
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

        private async Task TraverseDownTheStateHierarchyAndEnterStates(
            IStateLogic<TState, TEvent> stateLogic,
            Stack<IStateDefinition<TState, TEvent>> stack)
        {
            while (stack.Count > 0)
            {
                var state = stack.Pop();
                await stateLogic.Entry(state, this.context)
                    .ConfigureAwait(false);
            }
        }
    }
}