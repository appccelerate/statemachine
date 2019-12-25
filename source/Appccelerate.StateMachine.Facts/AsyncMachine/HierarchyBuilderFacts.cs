//-------------------------------------------------------------------------------
// <copyright file="HierarchyBuilderFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine
{
    using System;
    using System.Collections.Generic;
    using Appccelerate.StateMachine.AsyncMachine.Building;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class HierarchyBuilderFacts
    {
        private const string SuperState = "SuperState";
        private readonly HierarchyBuilder<string, int> testee;
        private readonly ImplicitAddIfNotAvailableStateDefinitionDictionary<string, int> states;
        private readonly Dictionary<string, BuildableStateDefinition<string, int>> initiallyLastActiveStates;

        public HierarchyBuilderFacts()
        {
            this.states = new ImplicitAddIfNotAvailableStateDefinitionDictionary<string, int>();
            this.initiallyLastActiveStates = new Dictionary<string, BuildableStateDefinition<string, int>>();

            this.testee = new HierarchyBuilder<string, int>(SuperState, this.states, this.initiallyLastActiveStates);
        }

        [Theory]
        [InlineData(HistoryType.Deep)]
        [InlineData(HistoryType.Shallow)]
        [InlineData(HistoryType.None)]
        public void SetsHistoryTypeOfSuperState(HistoryType historyType)
        {
            this.testee.WithHistoryType(historyType);

            this.states[SuperState].HistoryType
                .Should().Be(historyType);
        }

        [Fact]
        public void SetsInitialSubStateOfSuperState()
        {
            const string SubState = "SubState";

            this.testee.WithInitialSubState(SubState);

            this.states[SuperState].InitialState
                .Should().BeSameAs(this.states[SubState]);
        }

        [Fact]
        public void SettingTheInitialSubStateAlsoAddsItToTheInitiallyLastActiveStates()
        {
            const string SubState = "SubState";

            this.testee.WithInitialSubState(SubState);

            this.initiallyLastActiveStates[SuperState]
                .Should().Be(this.states[SubState]);
        }

        [Fact]
        public void AddsSubStatesToSuperState()
        {
            const string AnotherSubState = "AnotherSubState";

            this.testee
                .WithSubState(AnotherSubState);

            this.states[SuperState]
                .SubStates
                .Should()
                .HaveCount(1)
                .And
                .Contain(this.states[AnotherSubState]);
        }

        [Fact]
        public void ThrowsExceptionIfSubStateAlreadyHasASuperState()
        {
            const string SubState = "SubState";
            this.states[SubState].SuperState = this.states[SuperState];

            this.testee.Invoking(t => t.WithInitialSubState(SubState))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(BuildingExceptionMessages.CannotSetStateAsASuperStateBecauseASuperStateIsAlreadySet(
                    SuperState,
                    this.states[SubState]));
        }
    }
}