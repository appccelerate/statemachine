//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilderFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class HierarchyBuilderFacts
    {
        private const string SuperState = "SuperState";

        private readonly Machine.HierarchyBuilder<string, int> testee;

        private readonly Machine.IStateDictionary<string, int> states;

        private readonly Machine.IState<string, int> superState;

        public HierarchyBuilderFacts()
        {
            this.superState = A.Fake<Machine.IState<string, int>>();
            A.CallTo(() => this.superState.Id).Returns(SuperState);
            this.states = A.Fake<Machine.IStateDictionary<string, int>>();
            A.CallTo(() => this.states[SuperState]).Returns(this.superState);

            this.testee = new Machine.HierarchyBuilder<string, int>(this.states, SuperState);
        }

        [Theory]
        [InlineData(HistoryType.Deep)]
        [InlineData(HistoryType.Shallow)]
        [InlineData(HistoryType.None)]
        public void SetsHistoryTypeOfSuperState(HistoryType historyType)
        {
            this.testee.WithHistoryType(historyType);

            this.superState.HistoryType
                .Should().Be(historyType);
        }

        [Fact]
        public void SetsInitialSubStateOfSuperState()
        {
            const string SubState = "SubState";
            var subState = A.Fake<Machine.IState<string, int>>();
            subState.SuperState = null;
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.WithInitialSubState(SubState);

            this.superState.InitialState
                .Should().BeSameAs(subState);
        }

        [Fact]
        public void AddsSubStatesToSuperState()
        {
            const string AnotherSubState = "AnotherSubState";
            var anotherSubState = A.Fake<Machine.IState<string, int>>();
            anotherSubState.SuperState = null;
            A.CallTo(() => this.states[AnotherSubState]).Returns(anotherSubState);

            this.testee
                .WithSubState(AnotherSubState);

            A.CallTo(() => this.superState.SubStates.Add(anotherSubState)).MustHaveHappened();
        }

        [Fact]
        public void ThrowsExceptionIfSubStateAlreadyHasASuperState()
        {
            const string SubState = "SubState";
            var subState = A.Fake<Machine.IState<string, int>>();
            subState.SuperState = A.Fake<Machine.IState<string, int>>();
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.Invoking(t => t.WithInitialSubState(SubState))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage(Machine.ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                    SuperState,
                    subState));
        }
    }
}