//-------------------------------------------------------------------------------
// <copyright file="Builder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts
{
    using System;
    using FakeItEasy;
    using StateMachine.Machine;
    using StateMachine.Machine.GuardHolders;
    using StateMachine.Machine.States;

    public static class Builder<TState, TEvent>
            where TState : IComparable
            where TEvent : IComparable
    {
        public static GuardBuilder CreateGuardHolder()
        {
            return new GuardBuilder();
        }

        public static StateDefinitionBuilder CreateStateDefinition()
        {
            return new StateDefinitionBuilder();
        }

        public static TransitionContextBuilder CreateTransitionContext()
        {
            return new TransitionContextBuilder();
        }

        public class GuardBuilder
        {
            private readonly IGuardHolder guardHolder;

            public GuardBuilder()
            {
                this.guardHolder = A.Fake<IGuardHolder>();
            }

            public GuardBuilder ReturningTrue()
            {
                A.CallTo(() => this.guardHolder.Execute(A<object>._)).Returns(true);

                return this;
            }

            public GuardBuilder ReturningFalse()
            {
                A.CallTo(() => this.guardHolder.Execute(A<object>._)).Returns(false);

                return this;
            }

            public GuardBuilder Throwing(Exception exception)
            {
                A.CallTo(() => this.guardHolder.Execute(A<object>._)).Throws(exception);

                return this;
            }

            public IGuardHolder Build()
            {
                return this.guardHolder;
            }
        }

        public class StateDefinitionBuilder
        {
            private readonly IStateDefinition<TState, TEvent> stateDefinition;

            private IStateDefinition<TState, TEvent> superState;

            private int level;

            public StateDefinitionBuilder()
            {
                this.stateDefinition = A.Fake<IStateDefinition<TState, TEvent>>();
            }

            public StateDefinitionBuilder WithSuperState(IStateDefinition<TState, TEvent> newSuperState)
            {
                this.superState = newSuperState;
                this.level = newSuperState.Level + 1;

                return this;
            }

            public IStateDefinition<TState, TEvent> Build()
            {
                A.CallTo(() => this.stateDefinition.SuperState).Returns(this.superState);
                A.CallTo(() => this.stateDefinition.Level).Returns(this.level);

                return this.stateDefinition;
            }
        }

        public class TransitionContextBuilder
        {
            private readonly ITransitionContext<TState, TEvent> transitionContext;

            public TransitionContextBuilder()
            {
                this.transitionContext = A.Fake<ITransitionContext<TState, TEvent>>();
            }

            public TransitionContextBuilder WithStateDefinition(IStateDefinition<TState, TEvent> stateDefinition)
            {
                A.CallTo(() => this.transitionContext.StateDefinition).Returns(stateDefinition);
                return this;
            }

            public ITransitionContext<TState, TEvent> Build()
            {
                return this.transitionContext;
            }
        }
    }
}