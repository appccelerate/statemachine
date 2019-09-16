//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitionFacts.cs" company="Appccelerate">
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
    using StateMachine.AsyncMachine.States;
    using Xunit;

    public class HierarchicalTransitionFacts : SuccessfulTransitionWithExecutedActionsFactsBase
    {
        private readonly IStateDefinition<States, Events> root;
        private readonly IStateDefinition<States, Events> superStateOfSource;
        private readonly IStateDefinition<States, Events> superStateOfTarget;

        public HierarchicalTransitionFacts()
        {
            this.root = Builder<States, Events>.CreateStateDefinition().Build();
            this.superStateOfSource = Builder<States, Events>.CreateStateDefinition().WithSuperState(this.root).Build();
            this.Source = Builder<States, Events>.CreateStateDefinition().WithSuperState(this.superStateOfSource).Build();
            this.superStateOfTarget = Builder<States, Events>.CreateStateDefinition().WithSuperState(this.root).Build();
            this.Target = Builder<States, Events>.CreateStateDefinition().WithSuperState(this.superStateOfTarget).Build();

            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.TransitionDefinition.Source = this.Source;
            this.TransitionDefinition.Target = this.Target;
        }

        [Fact]
        public async Task ExitsStatesUpToBelowCommonSuperState()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Exit(this.Source, this.TransitionContext, this.LastActiveStateModifier)).MustHaveHappened()
                .Then(A.CallTo(() => this.StateLogic.Exit(this.superStateOfSource, this.TransitionContext, this.LastActiveStateModifier)).MustHaveHappened());
        }

        [Fact]
        public async Task EntersStatesBelowCommonSuperStateToTarget()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Entry(this.superStateOfTarget, this.TransitionContext)).MustHaveHappened()
                .Then(A.CallTo(() => this.StateLogic.Entry(this.Target, this.TransitionContext)).MustHaveHappened());
        }

        [Fact]
        public async Task DoesNotExitCommonSuperState()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Exit(this.root, this.TransitionContext, this.LastActiveStateModifier)).MustNotHaveHappened();
        }

        [Fact]
        public async Task DoesNotEnterCommonSuperState()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Entry(this.root, this.TransitionContext)).MustNotHaveHappened();
        }
    }
}