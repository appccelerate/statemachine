//-------------------------------------------------------------------------------
// <copyright file="ArgumentGuardHolderFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.GuardHolders
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.AsyncMachine.GuardHolders;
    using Xunit;

    public class ArgumentGuardHolderFacts
    {
        private readonly ArgumentGuardHolder<IBase> testee;

        private bool guardExecuted;

        public ArgumentGuardHolderFacts()
        {
            this.guardExecuted = false;
            this.testee = new ArgumentGuardHolder<IBase>(async v => await Guard(v));

            // ReSharper disable once UnusedParameter.Local
            Task<bool> Guard(IBase v) => Task.FromResult(this.guardExecuted = true);
        }

        public interface IBase
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public interface IDerived : IBase
        {
        }

        [Fact]
        public async Task Execute()
        {
            await this.testee.Execute(A.Fake<IBase>());

            this.guardExecuted
                .Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteWhenPassingADerivedClassThenGuardGetsExecuted()
        {
            await this.testee.Execute(A.Fake<IDerived>());

            this.guardExecuted
                .Should().BeTrue();
        }

        [Fact]
        public void ExecuteWhenPassingWrongTypeThenException()
        {
            const int argument = 4;

            Func<Task> action = async () => await this.testee.Execute(argument);

            action
                .Should().Throw<ArgumentException>()
                .WithMessage(StateMachine.Machine.GuardHolders.GuardHoldersExceptionMessages.CannotCastArgumentToGuardArgument(argument, this.testee.Describe()));
        }
    }
}