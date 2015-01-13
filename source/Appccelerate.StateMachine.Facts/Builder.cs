//-------------------------------------------------------------------------------
// <copyright file="Builder.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

namespace Appccelerate.StateMachine
{
    using System;
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Machine.GuardHolders;
    using FakeItEasy;

    public static class Builder<TState, TEvent>
            where TState : IComparable
            where TEvent : IComparable
    {
        public static GuardBuilder CreateGuardHolder()
        {
            return new GuardBuilder();
        }

        public static StateBuilder CreateState()
        {
            return new StateBuilder();   
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

        public class StateBuilder
        {
            private readonly IState<TState, TEvent> state;

            private IState<TState, TEvent> superState;

            private int level;

            public StateBuilder()
            {
                this.state = A.Fake<IState<TState, TEvent>>();
            }

            public StateBuilder WithSuperState(IState<TState, TEvent> newSuperState)
            {
                this.superState = newSuperState;
                this.level = newSuperState.Level + 1;

                return this;
            }

            public IState<TState, TEvent> Build()
            {
                A.CallTo(() => this.state.SuperState).Returns(this.superState);
                A.CallTo(() => this.state.Level).Returns(this.level);

                return this.state;
            }
        }

        public class TransitionContextBuilder
        {
            private readonly ITransitionContext<TState, TEvent> transitionContext;

            public TransitionContextBuilder()
            {
                this.transitionContext = A.Fake<ITransitionContext<TState, TEvent>>();
            }

            public TransitionContextBuilder WithState(IState<TState, TEvent> state)
            {
                A.CallTo(() => this.transitionContext.State).Returns(state);

                return this;
            }

            public ITransitionContext<TState, TEvent> Build()
            {
                return this.transitionContext;
            }
        }
    }
}