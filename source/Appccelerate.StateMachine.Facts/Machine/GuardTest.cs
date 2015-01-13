//-------------------------------------------------------------------------------
// <copyright file="GuardTest.cs" company="Appccelerate">
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
    /// Tests the guard feature of the <see cref="StateMachine"/>.
    /// </summary>
    public class GuardTest
    {
        private const string EventArgument = "test";

        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        public GuardTest()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();
        }

        [Fact]
        public void EventArgumentIsPassedToTheGuard()
        {
            string eventArgument = null;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.A)
                    .If<string>(argument =>
                        {
                            eventArgument = argument;
                            return true;
                        })
                    .Goto(StateMachine.States.B);

            this.testee.Fire(StateMachine.Events.A, EventArgument);

            eventArgument.Should().Be(EventArgument);
        }

        [Fact]
        public void GuardWithoutArguments()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                    .If(() => false).Goto(StateMachine.States.C)
                    .If(() => true).Goto(StateMachine.States.B);

            this.testee.Fire(StateMachine.Events.B);

            Assert.Equal(StateMachine.States.B, this.testee.CurrentStateId);
        }

        [Fact]
        public void GuardWithASingleArgument()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                    .If<int>(SingleIntArgumentGuardReturningFalse).Goto(StateMachine.States.C)
                    .If(() => false).Goto(StateMachine.States.D)
                    .If(() => false).Goto(StateMachine.States.E)
                    .If<int>(SingleIntArgumentGuardReturningTrue).Goto(StateMachine.States.B);

            this.testee.Fire(StateMachine.Events.B, 3);

            Assert.Equal(StateMachine.States.B, this.testee.CurrentStateId);
        }

        private static bool SingleIntArgumentGuardReturningTrue(int i)
        {
            return true;
        }

        private static bool SingleIntArgumentGuardReturningFalse(int i)
        {
            return false;
        }
    }
}