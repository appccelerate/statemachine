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
        [Fact]
        public async Task SyncActionIsInvokedWithSameArgumentThatIsPassedToGuardHolderExecuted()
        {
            var expected = new MyArgument();
            MyArgument value = null;
            bool SyncGuard(MyArgument x)
            {
                value = x;
                return true;
            }

            var testee = new ArgumentGuardHolder<MyArgument>(SyncGuard);

            await testee.Execute(expected);

            value.Should().Be(expected);
        }

        [Fact]
        public async Task AsyncActionIsInvokedWithSameArgumentThatIsPassedTGuardHolderExecuted()
        {
            var expected = new MyArgument();
            MyArgument value = null;
            Task<bool> AsyncGuard(MyArgument x)
            {
                value = x;
                return Task.FromResult(true);
            }

            var testee = new ArgumentGuardHolder<MyArgument>(AsyncGuard);

            await testee.Execute(expected);

            value.Should().Be(expected);
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousSyncActionWhenDescribing()
        {
            var testee = new ArgumentGuardHolder<MyArgument>(SyncGuard);

            var description = testee.Describe();

            description
                .Should()
                .Be("SyncGuard");
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ArgumentGuardHolder<MyArgument>(AsyncGuard);

            var description = testee.Describe();

            description
                .Should()
                .Be("AsyncGuard");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousSyncActionWhenDescribing()
        {
            var testee = new ArgumentGuardHolder<MyArgument>(a => true);

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ArgumentGuardHolder<MyArgument>(a => Task.FromResult(true));

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public async Task MatchingType()
        {
            var testee = new ArgumentGuardHolder<IBase>(BaseGuard);

            await testee.Execute(A.Fake<IBase>());
        }

        [Fact]
        public async Task DerivedType()
        {
            var testee = new ArgumentGuardHolder<IBase>(BaseGuard);

            await testee.Execute(A.Fake<IDerived>());
        }

        [Fact]
        public void NonMatchingType()
        {
            var testee = new ArgumentGuardHolder<IBase>(BaseGuard);

            Func<Task> action = async () => await testee.Execute(3);

            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(GuardHoldersExceptionMessages.CannotCastArgumentToGuardArgument(3, "BaseGuard"));
        }

        private static bool SyncGuard(MyArgument a)
        {
            return true;
        }

        private static Task<bool> AsyncGuard(MyArgument a)
        {
            return Task.FromResult(true);
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