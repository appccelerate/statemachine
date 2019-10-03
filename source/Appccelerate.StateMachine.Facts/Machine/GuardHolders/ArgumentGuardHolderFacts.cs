//-------------------------------------------------------------------------------
// <copyright file="ArgumentGuardHolderFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine.GuardHolders
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.Machine.GuardHolders;
    using Xunit;

    public class ArgumentGuardHolderFacts
    {
        [Fact]
        public void ActionIsInvokedWithSameArgumentThatIsPassedToGuardHolderExecuted()
        {
            var expected = new MyArgument();
            MyArgument value = null;
            bool Guard(MyArgument x)
            {
                value = x;
                return true;
            }

            var testee = new ArgumentGuardHolder<MyArgument>(Guard);

            testee.Execute(expected);

            value.Should().Be(expected);
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentGuardHolder<MyArgument>(Guard);

            var description = testee.Describe();

            description
                .Should()
                .Be("Guard");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentGuardHolder<MyArgument>(a => true);

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public void MatchingType()
        {
            var testee = new ArgumentGuardHolder<IBase>(BaseGuard);

            testee.Execute(A.Fake<IBase>());
        }

        [Fact]
        public void DerivedType()
        {
            var testee = new ArgumentGuardHolder<IBase>(BaseGuard);

            testee.Execute(A.Fake<IDerived>());
        }

        [Fact]
        public void NonMatchingType()
        {
            var testee = new ArgumentGuardHolder<IBase>(BaseGuard);

            Action action = () => testee.Execute(3);

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(GuardHoldersExceptionMessages.CannotCastArgumentToGuardArgument(3, "BaseGuard"));
        }

        private static bool Guard(MyArgument a)
        {
            return true;
        }

        private static bool BaseGuard(IBase b)
        {
            return true;
        }

        private class MyArgument
        {
        }

        public interface IBase
        {
        }

        public interface IDerived : IBase
        {
        }
    }
}