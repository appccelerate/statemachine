//-------------------------------------------------------------------------------
// <copyright file="StateActionFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using Xunit;
    
    public class StateActionFacts
    {
        private readonly StateMachine<Facts.States, Events> testee;

        public StateActionFacts()
        {
            this.testee = new StateMachine<Facts.States, Events>();
        }

        [Fact]
        public async Task EntryAction()
        {
            var entered = false;

            this.testee.In(States.A)
                .ExecuteOnEntry(() => entered = true);

            await this.testee.Initialize(States.A);

            await this.testee.EnterInitialState();

            entered.Should().BeTrue("entry action was not executed.");
        }

        [Fact]
        public async Task EntryActions()
        {
            var entered1 = false;
            var entered2 = false;

            this.testee.In(States.A)
                .ExecuteOnEntry(() => entered1 = true)
                .ExecuteOnEntry(() => entered2 = true);

            await this.testee.Initialize(States.A);

            await this.testee.EnterInitialState();

            entered1.Should().BeTrue("entry action was not executed.");
            entered2.Should().BeTrue("entry action was not executed.");
        }

        [Fact]
        public async Task ParameterizedEntryAction()
        {
            const int Parameter = 3;

            var receivedValue = 0;

            this.testee.In(States.A)
                .ExecuteOnEntryParametrized(parameter => receivedValue = parameter, Parameter);

            await this.testee.Initialize(States.A);

            await this.testee.EnterInitialState();

            receivedValue.Should().Be(Parameter);
        }

        [Fact]
        public async Task ExitAction()
        {
            var exit = false;

            this.testee.In(States.A)
                .ExecuteOnExit(() => exit = true)
                .On(Events.B).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, null);

            exit.Should().BeTrue("exit action was not executed.");
        }

        [Fact]
        public async Task ExitActions()
        {
            var exit1 = false;
            var exit2 = false;

            this.testee.In(States.A)
                .ExecuteOnExit(() => exit1 = true)
                .ExecuteOnExit(() => exit2 = true)
                .On(Events.B).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, null);

            exit1.Should().BeTrue("exit action was not executed.");
            exit2.Should().BeTrue("exit action was not executed.");
        }

        [Fact]
        public async Task ParametrizedExitAction()
        {
            const int Parameter = 3;

            var receivedValue = 0;

            this.testee.In(States.A)
                .ExecuteOnExitParametrized(value => receivedValue = value, Parameter)
                .On(Events.B).Goto(States.B);

            await this.testee.Initialize(States.A);
            await this.testee.EnterInitialState();

            await this.testee.Fire(Events.B, null);

            receivedValue.Should().Be(Parameter);
        }
    }
}