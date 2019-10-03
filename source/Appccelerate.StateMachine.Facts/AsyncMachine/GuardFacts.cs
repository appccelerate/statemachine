//-------------------------------------------------------------------------------
// <copyright file="GuardFacts.cs" company="Appccelerate">
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
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.Infrastructure;
    using Xunit;

    public class GuardFacts
    {
        [Fact]
        public async Task EventArgumentIsPassedToTheGuard_SyncUsage()
        {
            const string ExpectedEventArgument = "test";
            string actualEventArgument = null;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.A)
                    .If<string>(argument =>
                    {
                        actualEventArgument = argument;
                        return true;
                    })
                    .Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.A, ExpectedEventArgument, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            actualEventArgument
                .Should()
                .Be(ExpectedEventArgument);
        }

        [Fact]
        public async Task EventArgumentIsPassedToTheGuard_AsyncUsage()
        {
            const string ExpectedEventArgument = "test";
            string actualEventArgument = null;

            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.A)
                .If((string argument) =>
                {
                    actualEventArgument = argument;
                    return Task.FromResult(true);
                })
                .Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.A, ExpectedEventArgument, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            actualEventArgument
                .Should()
                .Be(ExpectedEventArgument);
        }

        [Fact]
        public async Task GuardWithoutArguments()
        {
            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                .On(Events.B)
                    .If(() => false).Goto(States.C)
                    .If(() => true).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, Missing.Value, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.B));
        }

        [Fact]
        public async Task GuardWithASingleArgument()
        {
            var stateDefinitionsBuilder = new StateDefinitionsBuilder<States, Events>();
            stateDefinitionsBuilder
                .In(States.A)
                    .On(Events.B)
                        .If((Func<int, bool>)SingleIntArgumentGuardReturningFalse).Goto(States.C)
                        .If(() => false).Goto(States.D)
                        .If(() => false).Goto(States.E)
                        .If((Func<int, bool>)SingleIntArgumentGuardReturningTrue).Goto(States.B);
            var stateDefinitions = stateDefinitionsBuilder.Build();

            var stateContainer = new StateContainer<States, Events>();
            var testee = new StateMachineBuilder<States, Events>()
                .WithStateContainer(stateContainer)
                .Build();

            await testee.EnterInitialState(stateContainer, stateDefinitions, States.A)
                .ConfigureAwait(false);

            await testee.Fire(Events.B, 3, stateContainer, stateDefinitions)
                .ConfigureAwait(false);

            stateContainer
                .CurrentStateId
                .Should()
                .BeEquivalentTo(Initializable<States>.Initialized(States.B));
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