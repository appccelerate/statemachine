//-------------------------------------------------------------------------------
// <copyright file="SingleArgumentGuardHolderTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.GuardHolders
{
    using System;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    public class SingleArgumentGuardHolderTest
    {
        private readonly ArgumentGuardHolder<IBase> testee;

        private bool guardExecuted;

        public SingleArgumentGuardHolderTest()
        {
            this.guardExecuted = false;
            Func<IBase, bool> guard = v => this.guardExecuted = true;
            this.testee = new ArgumentGuardHolder<IBase>(guard);
        }

        public interface IBase
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global because of FakeItEasy
        public interface IDerived : IBase
        {
        }

        [Fact]
        public void Execute()
        {
            this.testee.Execute(A.Fake<IBase>());

            this.guardExecuted
                .Should().BeTrue();
        }

        [Fact]
        public void ExecuteWhenPassingADerivedClassThenGuardGetsExecuted()
        {
            this.testee.Execute(A.Fake<IDerived>());

            this.guardExecuted
                .Should().BeTrue();
        }

        [Fact]
        public void ExecuteWhenPassingWrongTypeThenException()
        {
            const int Argument = 4;

            Action action = () => this.testee.Execute(Argument);

            action
                .ShouldThrow<ArgumentException>()
                .WithMessage(GuardHoldersExceptionMessages.CannotCastArgumentToGuardArgument(Argument, this.testee.Describe()));
        }
    }
}