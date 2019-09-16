//-------------------------------------------------------------------------------
// <copyright file="StateContainerFacts.cs" company="Appccelerate">
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
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.Infrastructure;
    using Xunit;

    public class StateContainerFacts
    {
        [Fact]
        public void ReturnsName()
        {
            const string Name = "container";
            var stateContainer = new StateContainer<string, int>(Name);

            stateContainer
                .Name
                .Should()
                .Be(Name);
        }

        [Fact]
        public void ReturnsNothingAsLastActiveStateWhenStateWasNeverSet()
        {
            var stateContainer = new StateContainer<string, int>();

            stateContainer.SetLastActiveStateFor("A", "helloWorld");

            stateContainer
                .GetLastActiveStateFor("B")
                .Should()
                .BeEquivalentTo(Optional<string>.Nothing());
        }

        [Fact]
        public void ReturnsStateXAsLastActiveStateWhenXWasSetBefore()
        {
            var stateContainer = new StateContainer<string, int>();

            stateContainer.SetLastActiveStateFor("A", "Z");

            stateContainer
                .GetLastActiveStateFor("A")
                .Should()
                .BeEquivalentTo(Optional<string>.Just("Z"));
        }

        [Fact]
        public async Task ExtensionsWhenExtensionsAreClearedThenNoExtensionIsRegistered()
        {
            var executed = false;
            var extension = A.Fake<IExtensionInternal<string, int>>();

            var testee = new StateContainer<string, int>();

            testee.Extensions.Add(extension);
            testee.Extensions.Clear();

            await testee.ForEach(e =>
                {
                    executed = true;
                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);

            executed
                .Should().BeFalse();
        }
    }
}