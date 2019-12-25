//-------------------------------------------------------------------------------
// <copyright file="StateFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.State
{
    using System;
    using Appccelerate.StateMachine.AsyncMachine.Building;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.AsyncMachine.States;
    using Xunit;

    public class StateFacts
    {
        [Fact]
        public void HierarchyWhenDefiningAStateAsItsOwnSuperStateThenAnExceptionIsThrown()
        {
            var testee = new BuildableStateDefinition<States, Events>(States.A);

            Action action = () => testee.SuperState = testee;

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(ExceptionMessages.StateCannotBeItsOwnSuperState(testee.ToString()));
        }

        [Fact]
        public void HierarchyWhenDefiningAStateAsItsOwnInitialSubStateThenAnExceptionIsThrown()
        {
            var testee = new BuildableStateDefinition<States, Events>(States.A);

            Action action = () => testee.InitialState = testee;

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(StatesExceptionMessages.StateCannotBeTheInitialSubStateToItself(testee.ToString()));
        }

        [Fact]
        public void HierarchyWhenDefiningAStateAAndAssigningAnInitialStateThatDoesntHaveStateAAsSuperStateThenAnExceptionIsThrown()
        {
            var testee = new BuildableStateDefinition<States, Events>(States.A);

            var initialState = A.Fake<BuildableStateDefinition<States, Events>>();
            initialState.SuperState = A.Fake<BuildableStateDefinition<States, Events>>();

            Action action = () => testee.InitialState = initialState;

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(StatesExceptionMessages.StateCannotBeTheInitialStateOfSuperStateBecauseItIsNotADirectSubState(initialState.ToString(), testee.ToString()));
        }

        [Fact]
        public void HierarchyWhenSettingLevelThenTheLevelOfAllChildrenIsUpdated()
        {
            const int level = 2;
            var testee = new BuildableStateDefinition<States, Events>(States.A);
            var subState = A.Fake<BuildableStateDefinition<States, Events>>();
            testee.SubStates.Add(subState);

            testee.Level = level;

            subState.Level
                .Should().Be(level + 1);
        }
    }
}