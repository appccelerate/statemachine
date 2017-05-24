//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitionTest.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using FakeItEasy;
    using Xunit;

    public class HierarchicalTransitionTest : SuccessfulTransitionWithExecutedActionsTestBase
    {
        private readonly IState<States, Events> root;
        private readonly IState<States, Events> superStateOfSource;
        private readonly IState<States, Events> superStateOfTarget;

        public HierarchicalTransitionTest()
        {
            this.root = Builder<States, Events>.CreateState().Build();
            this.superStateOfSource = Builder<States, Events>.CreateState().WithSuperState(this.root).Build();
            this.Source = Builder<States, Events>.CreateState().WithSuperState(this.superStateOfSource).Build();
            this.superStateOfTarget = Builder<States, Events>.CreateState().WithSuperState(this.root).Build();
            this.Target = Builder<States, Events>.CreateState().WithSuperState(this.superStateOfTarget).Build();

            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.Testee.Source = this.Source;
            this.Testee.Target = this.Target;
        }

        [Fact]
        public void ExitsStatesUpToBelowCommonSuperState()
        {
            this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.Source.Exit(this.TransitionContext)).MustHaveHappened()
                .Then(A.CallTo(() => this.superStateOfSource.Exit(this.TransitionContext)).MustHaveHappened());
        }

        [Fact]
        public void EntersStatesBelowCommonSuperStateToTarget()
        {
                this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.superStateOfTarget.Entry(this.TransitionContext)).MustHaveHappened()
                .Then(A.CallTo(() => this.Target.Entry(this.TransitionContext)).MustHaveHappened());
        }

        [Fact]
        public void DoesNotExitCommonSuperState()
        {
            this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.root.Exit(this.TransitionContext)).MustNotHaveHappened();
        }

        [Fact]
        public void DoesNotEnterCommonSuperState()
        {
            this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.root.Entry(this.TransitionContext)).MustNotHaveHappened();
        }
    }
}