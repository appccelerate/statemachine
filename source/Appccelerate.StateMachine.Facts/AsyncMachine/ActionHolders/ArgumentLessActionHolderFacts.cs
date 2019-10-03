//-------------------------------------------------------------------------------
// <copyright file="ArgumentLessActionHolderFacts.cs" company="Appccelerate">
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

    public class ArgumentLessActionHolderFacts
    {
        [Fact]
        public async Task SyncActionIsInvokedWhenActionHolderIsExecuted()
        {
            var wasExecuted = false;
            void SyncAction() => wasExecuted = true;

            var testee = new ArgumentLessActionHolder(SyncAction);

            await testee.Execute(null);

            wasExecuted
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task AsyncActionIsInvokedWhenActionHolderIsExecuted()
        {
            var wasExecuted = false;
            Task AsyncAction()
            {
                wasExecuted = true;
                return Task.CompletedTask;
            }

            var testee = new ArgumentLessActionHolder(AsyncAction);

            await testee.Execute(null);

            wasExecuted
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousSyncActionWhenDescribing()
        {
            var testee = new ArgumentLessActionHolder(SyncAction);

            var description = testee.Describe();

            description
                .Should()
                .Be("SyncAction");
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ArgumentLessActionHolder(AsyncAction);

            var description = testee.Describe();

            description
                .Should()
                .Be("AsyncAction");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousSyncActionWhenDescribing()
        {
            var testee = new ArgumentLessActionHolder(() => { });

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ArgumentLessActionHolder(() => Task.CompletedTask);

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        private static void SyncAction()
        {
        }

        private static Task AsyncAction()
        {
            return Task.CompletedTask;
        }
    }
}