//-------------------------------------------------------------------------------
// <copyright file="ParameterizedActionHolderFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.ActionHolders
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.AsyncMachine.ActionHolders;
    using Xunit;

    public class ParameterizedActionHolderFacts
    {
        [Fact]
        public async Task SyncActionIsInvokedWithSameArgumentThatIsPassedToConstructor()
        {
            var expected = new MyArgument();
            var wrong = new MyArgument();
            MyArgument value = null;
            void SyncAction(MyArgument x) => value = x;

            var testee = new ParametrizedActionHolder<MyArgument>(SyncAction, expected);

            await testee.Execute(wrong);

            value.Should().Be(expected);
        }

        [Fact]
        public async Task AsyncActionIsInvokedWithSameArgumentThatIsPassedToConstructor()
        {
            var expected = new MyArgument();
            var wrong = new MyArgument();
            MyArgument value = null;
            Task AsyncAction(MyArgument x)
            {
                value = x;
                return Task.CompletedTask;
            }

            var testee = new ParametrizedActionHolder<MyArgument>(AsyncAction, expected);

            await testee.Execute(wrong);

            value.Should().Be(expected);
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousSyncActionWhenDescribing()
        {
            var testee = new ParametrizedActionHolder<MyArgument>(SyncAction, new MyArgument());

            var description = testee.Describe();

            description
                .Should()
                .Be("SyncAction");
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ParametrizedActionHolder<MyArgument>(AsyncAction, new MyArgument());

            var description = testee.Describe();

            description
                .Should()
                .Be("AsyncAction");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousSyncActionWhenDescribing()
        {
            var testee = new ParametrizedActionHolder<MyArgument>(a => { }, new MyArgument());

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ParametrizedActionHolder<MyArgument>(a => Task.CompletedTask, new MyArgument());

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        private static void SyncAction(MyArgument a)
        {
        }

        private static Task AsyncAction(MyArgument a)
        {
            return Task.CompletedTask;
        }

        private class MyArgument
        {
        }
    }
}