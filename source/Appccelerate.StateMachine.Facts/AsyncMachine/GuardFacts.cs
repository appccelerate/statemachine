//-------------------------------------------------------------------------------
// <copyright file="GuardFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class GuardFacts
    {
        private const string EventArgument = "test";

        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        public GuardFacts()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();

            this.testee.Initialize(StateMachine.States.A);
        }

        [Fact]
        public async Task EventArgumentIsPassedToTheGuard_SyncUsage()
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

            await this.testee.EnterInitialState();
            await this.testee.Fire(StateMachine.Events.A, EventArgument);

            eventArgument.Should().Be(EventArgument);
        }

        [Fact]
        public async Task EventArgumentIsPassedToTheGuard_AsyncUsage()
        {
            string eventArgument = null;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.A)
                .If((string argument) =>
                {
                    eventArgument = argument;
                    return Task.FromResult(true);
                })
                .Goto(StateMachine.States.B);

            await this.testee.EnterInitialState();
            await this.testee.Fire(StateMachine.Events.A, EventArgument);

            eventArgument.Should().Be(EventArgument);
        }

        [Fact]
        public async Task GuardWithoutArguments()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                    .If(() => false).Goto(StateMachine.States.C)
                    .If(() => true).Goto(StateMachine.States.B);

            await this.testee.EnterInitialState();
            await this.testee.Fire(StateMachine.Events.B, Missing.Value);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public async Task GuardWithASingleArgument()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B)
                    .If((Func<int, bool>)SingleIntArgumentGuardReturningFalse).Goto(StateMachine.States.C)
                    .If(() => false).Goto(StateMachine.States.D)
                    .If(() => false).Goto(StateMachine.States.E)
                    .If((Func<int, bool>)SingleIntArgumentGuardReturningTrue).Goto(StateMachine.States.B);

            await this.testee.EnterInitialState();
            await this.testee.Fire(StateMachine.Events.B, 3);

            this.testee.CurrentStateId.Should().Be(StateMachine.States.B);
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