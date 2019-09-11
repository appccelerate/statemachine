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

namespace Appccelerate.StateMachine.Facts.Machine.ActionHolders
{
    using FluentAssertions;
    using StateMachine.Machine.ActionHolders;
    using Xunit;

    public class ArgumentLessActionHolderFacts
    {
        [Fact]
        public void ActionIsInvokedWhenActionHolderIsExecuted()
        {
            var wasExecuted = false;
            void AnAction() => wasExecuted = true;

            var testee = new ArgumentLessActionHolder(AnAction);

            testee.Execute(null);

            wasExecuted
                .Should()
                .BeTrue();
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentLessActionHolder(Action);

            var description = testee.Describe();

            description
                .Should()
                .Be("Action");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentLessActionHolder(() => { });

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        private static void Action()
        {
        }
    }
}