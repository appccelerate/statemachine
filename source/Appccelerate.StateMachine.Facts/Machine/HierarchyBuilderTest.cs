//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilderTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using Xunit;

    public class HierarchyBuilderTest
    {
        private const string SuperState = "SuperState";
        private readonly HierarchyBuilder<string, int> testee;
        private readonly IImplicitAddIfNotAvailableStateDefinitionDictionary<string, int> states;
        private readonly StateDefinition<string, int> superState;
        private readonly IDictionary<string, string> initiallyLastActiveStates;

        public HierarchyBuilderTest()
        {
            this.superState = new StateDefinition<string, int>(SuperState);
            this.states = A.Fake<IImplicitAddIfNotAvailableStateDefinitionDictionary<string, int>>();
            A.CallTo(() => this.states[SuperState]).Returns(this.superState);
            this.initiallyLastActiveStates = A.Fake<IDictionary<string, string>>();

            this.testee = new HierarchyBuilder<string, int>(SuperState, this.states, this.initiallyLastActiveStates);
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
            var subState = new StateDefinition<string, int>(SubState)
            {
                SuperStateModifiable = null
            };
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.WithInitialSubState(SubState);

            this.superState.InitialState
                .Should().BeSameAs(subState);
        }

        [Fact]
        public void SettingTheInitialSubStateAlsoAddsItToTheInitiallyLastActiveStates()
        {
            const string SubState = "SubState";
            var subState = new StateDefinition<string, int>(SubState)
            {
                SuperStateModifiable = null
            };
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.WithInitialSubState(SubState);

            A.CallTo(() => this.initiallyLastActiveStates.Add(SuperState, subState.Id)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void AddsSubStatesToSuperState()
        {
            const string AnotherSubState = "AnotherSubState";
            var anotherSubState = new StateDefinition<string, int>(AnotherSubState)
            {
                SuperStateModifiable = null
            };
            A.CallTo(() => this.states[AnotherSubState]).Returns(anotherSubState);

            this.testee
                .WithSubState(AnotherSubState);

            this.superState
                .SubStates
                .Should()
                .HaveCount(1)
                .And
                .Contain(anotherSubState);
        }

        [Fact]
        public void ThrowsExceptionIfSubStateAlreadyHasASuperState()
        {
            const string SubState = "SubState";
            var subState = new StateDefinition<string, int>(SubState)
            {
                SuperStateModifiable = new StateDefinition<string, int>("SomeOtherSuperState")
            };
            A.CallTo(() => this.states[SubState]).Returns(subState);

            this.testee.Invoking(t => t.WithInitialSubState(SubState))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                    SuperState,
                    subState));
        }
    }
}