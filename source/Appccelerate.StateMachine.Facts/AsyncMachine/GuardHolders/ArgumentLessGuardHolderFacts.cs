//-------------------------------------------------------------------------------
// <copyright file="ArgumentLessGuardHolderFacts.cs" company="Appccelerate">
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
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.AsyncMachine.GuardHolders;
    using Xunit;

    public class ArgumentLessGuardHolderFacts
    {
        [Fact]
        public async Task SyncActionIsInvokedWhenGuardHolderIsExecuted()
        {
            var wasExecuted = false;
            bool SyncGuard()
            {
                wasExecuted = true;
                return true;
            }

            var testee = new ArgumentLessGuardHolder(SyncGuard);

            await testee.Execute(null);

            wasExecuted
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task AsyncActionIsInvokedWheGuardHolderIsExecuted()
        {
            var wasExecuted = false;
            Task<bool> SyncGuard()
            {
                wasExecuted = true;
                return Task.FromResult(true);
            }

            var testee = new ArgumentLessGuardHolder(SyncGuard);

            await testee.Execute(null);

            wasExecuted
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousSyncActionWhenDescribing()
        {
            var testee = new ArgumentLessGuardHolder(SyncGuard);

            var description = testee.Describe();

            description
                .Should()
                .Be("SyncGuard");
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ArgumentLessGuardHolder(AsyncGuard);

            var description = testee.Describe();

            description
                .Should()
                .Be("AsyncGuard");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousSyncActionWhenDescribing()
        {
            var testee = new ArgumentLessGuardHolder(() => true);

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousAsyncActionWhenDescribing()
        {
            var testee = new ArgumentLessGuardHolder(() => Task.FromResult(true));

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        private static bool SyncGuard()
        {
            return true;
        }

        private static Task<bool> AsyncGuard()
        {
            return Task.FromResult(true);
        }
    }
}