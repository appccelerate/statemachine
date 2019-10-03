//-------------------------------------------------------------------------------
// <copyright file="TransitionDefinedInSuperStateTransitionFacts.cs" company="Appccelerate">
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

    public class TransitionDefinedInSuperStateTransitionFacts : TransitionFactsBase
    {
        private readonly IStateDefinition<States, Events> intermediate;
        private readonly IStateDefinition<States, Events> current;

        public TransitionDefinedInSuperStateTransitionFacts()
        {
            this.Source = Builder<States, Events>.CreateStateDefinition().Build();
            this.intermediate = Builder<States, Events>.CreateStateDefinition().WithSuperState(this.Source).Build();
            this.current = Builder<States, Events>.CreateStateDefinition().WithSuperState(this.intermediate).Build();
            this.Target = Builder<States, Events>.CreateStateDefinition().Build();
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.current).Build();

            this.TransitionDefinition.Source = this.Source;
            this.TransitionDefinition.Target = this.Target;
        }

        [Fact]
        public async Task ExitsAllStatesFromCurrentUpToSource()
        {
            await this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            A.CallTo(() => this.StateLogic.Exit(this.current, this.TransitionContext, this.LastActiveStateModifier)).MustHaveHappened()
                .Then(A.CallTo(() => this.StateLogic.Exit(this.intermediate, this.TransitionContext, this.LastActiveStateModifier)).MustHaveHappened())
                .Then(A.CallTo(() => this.StateLogic.Exit(this.Source, this.TransitionContext, this.LastActiveStateModifier)).MustHaveHappened());
        }
    }
}