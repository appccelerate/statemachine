//-------------------------------------------------------------------------------
// <copyright file="TransitionsTest.cs" company="Appccelerate">
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
    using StateMachine.Infrastructure;
    using StateMachine.Machine;
    using Xunit;

    /// <summary>
    /// Tests transition behavior.
    /// </summary>
    public class TransitionsTest
    {
        /// <summary>
        /// When no transition for the fired event can be found in the entire
        /// hierarchy up from the current state then the transition declined event is fired and
        /// the state machine remains in its current state.
        /// </summary>
        [Fact]
        public void MissingTransition()
        {
            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            var declined = false;

            testee.TransitionDeclined += (sender, e) => { declined = true; };

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.C, stateContainer, stateContainer, stateDefinitions);

            declined.Should().BeTrue("Declined event was not fired");
            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.A));
        }

        /// <summary>
        /// Actions on transitions are performed and the event arguments are passed to them.
        /// </summary>
        [Fact]
        public void ExecuteActions()
        {
            const int EventArgument = 17;

            int? action1Argument = null;
            int? action2Argument = null;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B)
                    .Execute<int>(argument => { action1Argument = argument; })
                    .Execute((int argument) => { action2Argument = argument; });
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, EventArgument, stateContainer, stateDefinitions);

            action1Argument.Should().Be(EventArgument);
            action2Argument.Should().Be(EventArgument);
        }

        [Fact]
        public void ExecutesActions_WhenActionsWithAndWithoutArgumentAreDefined()
        {
            const int EventArgument = 17;

            var action1Executed = false;
            var action2Executed = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Goto(States.B)
                    .Execute<int>(argument => { action1Executed = true; })
                    .Execute((int argument) => { action2Executed = true; });
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, EventArgument, stateContainer, stateDefinitions);

            action1Executed.Should().BeTrue("action with argument should be executed");
            action2Executed.Should().BeTrue("action without argument should be executed");
        }

        /// <summary>
        /// Internal transitions can be executed
        /// (internal transition = transition that remains in the same state and does not execute exit
        /// and entry actions.
        /// </summary>
        [Fact]
        public void InternalTransition()
        {
            var executed = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.A)
                    .Execute(() => executed = true);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.A, stateContainer, stateContainer, stateDefinitions);

            executed.Should().BeTrue("internal transition was not executed.");
            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.A));
        }

        [Fact]
        public void ActionsWithoutArguments()
        {
            var executed = false;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Execute(() => executed = true);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            executed.Should().BeTrue();
        }

        [Fact]
        public void ActionsWithOneArgument()
        {
            const int ExpectedValue = 1;
            var value = 0;

            var stateDefinitionBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionBuilder
                .In(States.A)
                    .On(Events.B)
                    .Execute<int>(v => value = v);
            var stateDefinitions = stateDefinitionBuilder.Build();
            var stateContainer = new StateContainer<States, Events>();

            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.EnterInitialState(stateContainer, stateDefinitions, States.A);

            testee.Fire(Events.B, ExpectedValue, stateContainer, stateDefinitions);

            value.Should().Be(ExpectedValue);
        }
    }
}