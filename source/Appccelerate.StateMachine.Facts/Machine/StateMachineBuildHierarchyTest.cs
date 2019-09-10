//-------------------------------------------------------------------------------
// <copyright file="StateMachineBuildHierarchyTest.cs" company="Appccelerate">
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
    using FluentAssertions;
    using StateMachine.Machine;
    using Xunit;

    /// <summary>
    /// Tests hierarchy building in the <see cref="StateMachine{TState,TEvent}"/>.
    /// </summary>
    public class StateMachineBuildHierarchyTest
    {
        /// <summary>
        /// If the super-state is specified as the initial state of its sub-states then an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        [Fact]
        public void AddHierarchicalStatesInitialStateIsSuperStateItself()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();

            Action a = () =>
                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(States.B)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(States.B)
                        .WithSubState(States.B1)
                        .WithSubState(States.B2);
            a.Should().Throw<ArgumentException>();
        }
    }
}