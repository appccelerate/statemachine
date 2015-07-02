//-------------------------------------------------------------------------------
// <copyright file="StateMachineBuildHierarchyTest.cs" company="Appccelerate">
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

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests hierarchy building in the <see cref="StateMachine{TState,TEvent}"/>.
    /// </summary>
    public class StateMachineBuildHierarchyTest
    {
        /// <summary>
        /// Object under test.
        /// </summary>
        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineBuildHierarchyTest"/> class.
        /// </summary>
        public StateMachineBuildHierarchyTest()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();
        }

        /// <summary>
        /// If the super-state is specified as the initial state of its sub-states then an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        [Fact]
        public void AddHierarchicalStatesInitialStateIsSuperStateItself()
        {
            Action action = () => this.testee.DefineHierarchyOn(StateMachine.States.B)
                                  .WithHistoryType(HistoryType.None)
                                  .WithInitialSubState(StateMachine.States.B)
                                  .WithSubState(StateMachine.States.B1)
                                  .WithSubState(StateMachine.States.B2);

            action.ShouldThrow<ArgumentException>();
        }
    }
}