//-------------------------------------------------------------------------------
// <copyright file="StateActionFacts.cs" company="Appccelerate">
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
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using Xunit;

    public class StateActionFacts
    {
        [Fact]
        public async Task EntryAction()
        {
            var entered = false;

            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                .ExecuteOnEntry(() => entered = true);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            entered
                .Should()
                .BeTrue("entry action was not executed.");
        }

        [Fact]
        public async Task EntryActions()
        {
            var entered1 = false;
            var entered2 = false;

            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                .ExecuteOnEntry(() => entered1 = true)
                .ExecuteOnEntry(() => entered2 = true);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            entered1.Should().BeTrue("entry action was not executed.");
            entered2.Should().BeTrue("entry action was not executed.");
        }

        [Fact]
        public async Task ParameterizedEntryAction()
        {
            const int Parameter = 3;

            var receivedValue = 0;

            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                .ExecuteOnEntryParametrized(parameter => receivedValue = parameter, Parameter);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            receivedValue
                .Should()
                .Be(Parameter);
        }

        [Fact]
        public async Task ExitAction()
        {
            var exit = false;

            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                .ExecuteOnExit(() => exit = true)
                .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, null, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            exit
                .Should()
                .BeTrue("exit action was not executed.");
        }

        [Fact]
        public async Task ExitActions()
        {
            var exit1 = false;
            var exit2 = false;

            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                .ExecuteOnExit(() => exit1 = true)
                .ExecuteOnExit(() => exit2 = true)
                .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, null, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            exit1.Should().BeTrue("exit action was not executed.");
            exit2.Should().BeTrue("exit action was not executed.");
        }

        [Fact]
        public async Task ParametrizedExitAction()
        {
            const int Parameter = 3;

            var receivedValue = 0;

            var stateContainer = new StateContainer<States, Events>();
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                .ExecuteOnExitParametrized(value => receivedValue = value, Parameter)
                .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, null, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            receivedValue.Should().Be(Parameter);
        }
    }
}