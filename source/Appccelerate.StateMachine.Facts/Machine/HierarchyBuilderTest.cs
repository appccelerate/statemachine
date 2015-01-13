//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilderTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;
    using Xunit.Extensions;

    public class HierarchyBuilderTest
    {
        private const string SuperState = "SuperState";

        private readonly HierarchyBuilder<string, int> testee;

        private readonly IStateDictionary<string, int> states;

        private readonly IState<string, int> superState;

        public HierarchyBuilderTest()
        {
            this.superState = A.Fake<IState<string, int>>();
            A.CallTo(() => this.superState.Id).Returns(SuperState);
            this.states = A.Fake<IStateDictionary<string, int>>();
            A.CallTo(() => this.states[SuperState]).Returns(this.superState);

            this.testee = new HierarchyBuilder<string, int>(this.states, SuperState);
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
            var subState = A.Fake<IState<string, int>>();
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
            var anotherSubState = A.Fake<IState<string, int>>();
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
            var subState = A.Fake<IState<string, int>>();
            subState.SuperState = A.Fake<IState<string, int>>(); 
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.Invoking(t => t.WithInitialSubState(SubState))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage(ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                    SuperState,
                    subState));
        }
    }
}