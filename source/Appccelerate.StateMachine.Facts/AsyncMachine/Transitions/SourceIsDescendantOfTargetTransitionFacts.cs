//-------------------------------------------------------------------------------
// <copyright file="SourceIsDescendantOfTargetTransitionFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.Transitions
{
    using System.Threading.Tasks;
    using AsyncMachine;
    using FakeItEasy;
    using StateMachine.AsyncMachine;
    using Xunit;

    public class SourceIsDescendantOfTargetTransitionFacts : SuccessfulTransitionWithExecutedActionsFactsBase
    {
        private readonly IState<States, Events> intermediate;

        public SourceIsDescendantOfTargetTransitionFacts()
        {
            this.Target = Builder<States, Events>.CreateState().Build();
            this.intermediate = Builder<States, Events>.CreateState().WithSuperState(this.Target).Build();
            this.Source = Builder<States, Events>.CreateState().WithSuperState(this.intermediate).Build();
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.Testee.Source = this.Source;
            this.Testee.Target = this.Target;
        }

        [Fact]
        public async Task ExitsOfAllStatesFromSourceUpToTarget()
        {
            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.Source.Exit(this.TransitionContext)).MustHaveHappened()
                .Then(A.CallTo(() => this.intermediate.Exit(this.TransitionContext)).MustHaveHappened())
                .Then(A.CallTo(() => this.Target.Exit(this.TransitionContext)).MustHaveHappened());
        }

        [Fact]
        public async Task EntersTargetState()
        {
            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.Target.Entry(this.TransitionContext)).MustHaveHappened();
        }
    }
}