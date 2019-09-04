//-------------------------------------------------------------------------------
// <copyright file="GuardsTransitionFacts.cs" company="Appccelerate">
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
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.AsyncMachine.GuardHolders;
    using Xunit;

    public class GuardsTransitionFacts : TransitionFactsBase
    {
        public GuardsTransitionFacts()
        {
            this.Source = Builder<States, Events>.CreateState().Build();
            this.Target = Builder<States, Events>.CreateState().Build();
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.Testee.Source = this.Source;
            this.Testee.Target = this.Target;
        }

        [Fact]
        public async Task Executes_WhenGuardIsMet()
        {
            var guard = Builder<States, Events>.CreateGuardHolder().ReturningTrue().Build();
            this.Testee.Guard = guard;

            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.Target.Entry(this.TransitionContext)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DoesNotExecute_WhenGuardIsNotMet()
        {
            var guard = Builder<States, Events>.CreateGuardHolder().ReturningFalse().Build();
            this.Testee.Guard = guard;

            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.Target.Entry(this.TransitionContext)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ReturnsNotFiredTransitionResult_WhenGuardIsNotMet()
        {
            var guard = Builder<States, Events>.CreateGuardHolder().ReturningFalse().Build();
            this.Testee.Guard = guard;

            ITransitionResult<States, Events> result = await this.Testee.Fire(this.TransitionContext);

            result.Should().BeNotFiredTransitionResult<States, Events>();
        }

        [Fact]
        public async Task NotifiesExtensions_WhenGuardIsNotMet()
        {
            var extension = A.Fake<IExtension<States, Events>>();
            this.ExtensionHost.Extension = extension;

            IGuardHolder guard = Builder<States, Events>.CreateGuardHolder().ReturningFalse().Build();
            this.Testee.Guard = guard;

            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => extension.SkippedTransition(
                this.StateMachineInformation,
                A<ITransition<States, Events>>.That.Matches(t => t.Source == this.Source && t.Target == this.Target),
                this.TransitionContext)).MustHaveHappened();
        }
    }
}