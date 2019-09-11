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

namespace Appccelerate.StateMachine.Facts.Machine.GuardHolders
{
    using FluentAssertions;
    using StateMachine.Machine.GuardHolders;
    using Xunit;

    public class ArgumentLessGuardHolderFacts
    {
        [Fact]
        public void ActionIsInvokedWhenGuardHolderIsExecuted()
        {
            var wasExecuted = false;
            bool Guard()
            {
                wasExecuted = true;
                return true;
            }

            var testee = new ArgumentLessGuardHolder(Guard);

            testee.Execute(null);

            wasExecuted
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentLessGuardHolder(Guard);

            var description = testee.Describe();

            description
                .Should()
                .Be("Guard");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentLessGuardHolder(() => true);

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        private static bool Guard()
        {
            return true;
        }
    }
}