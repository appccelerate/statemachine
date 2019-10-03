//-------------------------------------------------------------------------------
// <copyright file="InternalTransitionFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.Transitions
{
    using System.Threading.Tasks;
    using AsyncMachine;
    using FakeItEasy;
    using Xunit;

    public class InternalTransitionFacts : SuccessfulTransitionWithExecutedActionsFactsBase
    {
        public InternalTransitionFacts()
        {
            this.Source = Builder<States, Events>.CreateStateDefinition().Build();
            this.Target = this.Source;
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.TransitionDefinition.Source = this.Source;
            this.TransitionDefinition.Target = null; // target == null indicates an internal transition
        }

        [Fact]
        public async Task DoesNotExitState()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Exit(this.Source, this.TransitionContext, this.LastActiveStateModifier)).MustNotHaveHappened();
        }

        [Fact]
        public async Task DoesNotEnterState()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Entry(this.Source, this.TransitionContext)).MustNotHaveHappened();
        }
    }
}