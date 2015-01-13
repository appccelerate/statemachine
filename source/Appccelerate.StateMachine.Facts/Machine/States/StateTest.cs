//-------------------------------------------------------------------------------
// <copyright file="StateTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.States
{
    using System;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    using Events = Appccelerate.StateMachine.Events;
    using States = Appccelerate.StateMachine.States;

    public class StateTest
    {
        private readonly State<States, Events> testee;

        public StateTest()
        {
            this.testee = new State<States, Events>(
                States.A, 
                A.Fake<IStateMachineInformation<States, Events>>(), 
                A.Fake<IExtensionHost<States, Events>>());
        }

        [Fact]
        public void HierarchyWhenDefiningAStateAsItsOwnSuperStateThenAnExceptionIsThrown()
        {
            Action action = () => this.testee.SuperState = this.testee;

            action
                .ShouldThrow<ArgumentException>().WithMessage(ExceptionMessages.StateCannotBeItsOwnSuperState(this.testee.ToString()));
        }

        [Fact]
        public void HierarchyWhenSettingLevelThenTheLevelOfAllChildrenIsUpdated()
        {
            const int Level = 2;

            var subState = A.Fake<IState<States, Events>>();

            this.testee.SubStates.Add(subState);

            this.testee.Level = Level;

            subState.Level
                .Should().Be(Level + 1);
        }
    }
}