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

namespace Appccelerate.StateMachine.Facts.Machine.ActionHolders
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using StateMachine.Machine.ActionHolders;
    using Xunit;

    public class ParameterizedActionHolderFacts
    {
        [Fact]
        public void ActionIsInvokedWithSameArgumentThatIsPassedToConstructor()
        {
            var expected = new MyArgument();
            var wrong = new MyArgument();
            MyArgument value = null;
            void AnAction(MyArgument x) => value = x;

            var testee = new ParametrizedActionHolder<MyArgument>(AnAction, expected);

            testee.Execute(wrong);

            value.Should().Be(expected);
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousActionWhenDescribing()
        {
            var testee = new ParametrizedActionHolder<MyArgument>(Action, new MyArgument());

            var description = testee.Describe();

            description
                .Should()
                .Be("Action");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousActionWhenDescribing()
        {
            var testee = new ParametrizedActionHolder<MyArgument>(a => { }, new MyArgument());

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        private static void Action(MyArgument a)
        {
        }

        private class MyArgument
        {
        }
    }
}