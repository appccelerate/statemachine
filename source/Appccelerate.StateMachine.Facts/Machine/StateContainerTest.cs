//-------------------------------------------------------------------------------
// <copyright file="MissableTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine
{
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using Xunit;

    public class StateContainerTest
    {
        [Fact]
        public void ReturnsName()
        {
            const string Name = "container";
            var stateContainer = new StateContainer<string, int>(Name);

            stateContainer.Name.Should().Be(Name);
        }

        [Fact]
        public void ReturnsNullAsLastActiveStateWhenStateWasNeverSet()
        {
            var stateContainer = new StateContainer<string, int>("container");

            stateContainer.SetLastActiveStateFor("A", A.Fake<IStateDefinition<string, int>>());

            stateContainer.GetLastActiveStateOrNullFor("B").Should().BeNull();
        }

        [Fact]
        public void ReturnsStateDefinitionXAsLastActiveStateWhenStateDefinitionXWasSetBefore()
        {
            var stateContainer = new StateContainer<string, int>("container");
            var lastActiveState = A.Fake<IStateDefinition<string, int>>();

            stateContainer.SetLastActiveStateFor("A", lastActiveState);

            stateContainer.GetLastActiveStateOrNullFor("A").Should().Be(lastActiveState);
        }
    }
}