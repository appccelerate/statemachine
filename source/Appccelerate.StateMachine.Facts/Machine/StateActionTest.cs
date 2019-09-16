//-------------------------------------------------------------------------------
// <copyright file="StateActionTest.cs" company="Appccelerate">
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
    using FluentAssertions;
    using StateMachine.Machine;
    using Xunit;

    /// <summary>
    /// Tests that entry and exit actions are executed correctly.
    /// </summary>
    public class StateActionTest
    {
        [Fact]
        public void EntryAction()
        {
            var entered = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnEntry(() => entered = true);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            entered.Should().BeTrue("entry action was not executed.");
        }

        [Fact]
        public void EntryActions()
        {
            var entered1 = false;
            var entered2 = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnEntry(() => entered1 = true)
                    .ExecuteOnEntry(() => entered2 = true);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            entered1.Should().BeTrue("entry action was not executed.");
            entered2.Should().BeTrue("entry action was not executed.");
        }

        [Fact]
        public void ParameterizedEntryAction()
        {
            const int Parameter = 3;

            var receivedValue = 0;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnEntryParametrized(parameter => receivedValue = parameter, Parameter);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            receivedValue.Should().Be(Parameter);
        }

        [Fact]
        public void ExitAction()
        {
            var exit = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnExit(() => exit = true)
                    .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            exit.Should().BeTrue("exit action was not executed.");
        }

        [Fact]
        public void ExitActions()
        {
            var exit1 = false;
            var exit2 = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnExit(() => exit1 = true)
                    .ExecuteOnExit(() => exit2 = true)
                    .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            exit1.Should().BeTrue("exit action was not executed.");
            exit2.Should().BeTrue("exit action was not executed.");
        }

        [Fact]
        public void ParametrizedExitAction()
        {
            const int Parameter = 3;

            var receivedValue = 0;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .ExecuteOnExitParametrized(value => receivedValue = value, Parameter)
                    .On(Events.B).Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            receivedValue.Should().Be(Parameter);
        }
    }
}