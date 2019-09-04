//-------------------------------------------------------------------------------
// <copyright file="GuardTest.cs" company="Appccelerate">
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
    using FluentAssertions;
    using StateMachine.Machine;
    using Xunit;

    /// <summary>
    /// Tests the guard feature of the <see cref="StateMachine"/>.
    /// </summary>
    public class GuardTest
    {
        [Fact]
        public void EventArgumentIsPassedToTheGuard()
        {
            const string EventArgument = "test";
            string actualEventArgument = null;

            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.A)
                        .If<string>(argument =>
                        {
                            actualEventArgument = argument;
                            return true;
                        })
                        .Goto(StateMachine.States.B))
                .Build();
            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.A, EventArgument, stateContainer, stateContainer, stateDefinitions);

            actualEventArgument.Should().Be(EventArgument);
        }

        [Fact]
        public void GuardWithoutArguments()
        {
            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B)
                        .If(() => false).Goto(StateMachine.States.C)
                        .If(() => true).Goto(StateMachine.States.B))
                .Build();
            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.B, stateContainer, stateContainer, stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B);
        }

        [Fact]
        public void GuardWithASingleArgument()
        {
            var stateDefinitions = new StateDefinitionsBuilder<StateMachine.States, Events>()
                .WithConfiguration(x =>
                    x.In(StateMachine.States.A)
                        .On(Events.B)
                        .If<int>(SingleIntArgumentGuardReturningFalse).Goto(StateMachine.States.C)
                        .If(() => false).Goto(StateMachine.States.D)
                        .If(() => false).Goto(StateMachine.States.E)
                        .If<int>(SingleIntArgumentGuardReturningTrue).Goto(StateMachine.States.B))
                .Build();
            var stateContainer = new StateContainer<StateMachine.States, Events>();

            var testee = new StateMachineBuilder<StateMachine.States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            testee.Initialize(StateMachine.States.A, stateContainer, stateContainer);
            testee.EnterInitialState(stateContainer, stateContainer, stateDefinitions);

            testee.Fire(Events.B, 3, stateContainer, stateContainer, stateDefinitions);

            stateContainer.CurrentStateId.Should().Be(StateMachine.States.B);
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