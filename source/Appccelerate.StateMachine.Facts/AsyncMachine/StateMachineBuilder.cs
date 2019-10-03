//-------------------------------------------------------------------------------
// <copyright file="StateMachineBuilder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine
{
    using System;
    using StateMachine.AsyncMachine;
    using StateMachine.AsyncMachine.States;
    using StateMachine.AsyncMachine.Transitions;

    public class StateMachineBuilder<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private StateContainer<TState, TEvent> stateContainer;

        public StateMachineBuilder()
        {
            this.stateContainer = new StateContainer<TState, TEvent>();
        }

        public StateMachineBuilder<TState, TEvent> WithStateContainer(StateContainer<TState, TEvent> stateContainerToUse)
        {
            this.stateContainer = stateContainerToUse;
            return this;
        }

        public StateMachine<TState, TEvent> Build()
        {
            var factory = new StandardFactory<TState, TEvent>();
            var transitionLogic = new TransitionLogic<TState, TEvent>(this.stateContainer);
            var stateLogic = new StateLogic<TState, TEvent>(transitionLogic, this.stateContainer);
            transitionLogic.SetStateLogic(stateLogic);

            return new StateMachine<TState, TEvent>(factory, stateLogic);
        }
    }
}
