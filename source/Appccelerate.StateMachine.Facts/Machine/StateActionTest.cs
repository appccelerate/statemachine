//-------------------------------------------------------------------------------
// <copyright file="StateActionTest.cs" company="Appccelerate">
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
    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests that entry and exit actions are executed correctly.
    /// </summary>
    public class StateActionTest
    {
        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        public StateActionTest()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();
        }

        [Fact]
        public void EntryAction()
        {
            bool entered = false;

            this.testee.In(StateMachine.States.A)
                .ExecuteOnEntry(() => entered = true);

            this.testee.Initialize(StateMachine.States.A);

            this.testee.EnterInitialState();

            Assert.True(entered, "entry action was not executed.");
        }

        [Fact]
        public void EntryActions()
        {
            bool entered1 = false;
            bool entered2 = false;

            this.testee.In(StateMachine.States.A)
                .ExecuteOnEntry(() => entered1 = true)
                .ExecuteOnEntry(() => entered2 = true);

            this.testee.Initialize(StateMachine.States.A);

            this.testee.EnterInitialState();

            entered1.Should().BeTrue("entry action was not executed.");
            entered2.Should().BeTrue("entry action was not executed.");
        }

        [Fact]
        public void ParameterizedEntryAction()
        {
            int i = 0;

            this.testee.In(StateMachine.States.A)
                .ExecuteOnEntryParametrized(parameter => i = parameter, 3);

            this.testee.Initialize(StateMachine.States.A);

            this.testee.EnterInitialState();

            Assert.Equal(3, i);
        }

        [Fact]
        public void ExitAction()
        {
            bool exit = false;

            this.testee.In(StateMachine.States.A)
                .ExecuteOnExit(() => exit = true)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B);

            Assert.True(exit, "exit action was not executed.");
        }

        [Fact]
        public void ExitActions()
        {
            bool exit1 = false;
            bool exit2 = false;

            this.testee.In(StateMachine.States.A)
                .ExecuteOnExit(() => exit1 = true)
                .ExecuteOnExit(() => exit2 = true)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B);

            exit1.Should().BeTrue("exit action was not executed.");
            exit2.Should().BeTrue("exit action was not executed.");
        }

        [Fact]
        public void ParametrizedExitAction()
        {
            int i = 0;

            this.testee.In(StateMachine.States.A)
                .ExecuteOnExitParametrized(value => i = value, 3)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B);

            Assert.Equal(i, 3);
        }
    }
}