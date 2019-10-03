//-------------------------------------------------------------------------------
// <copyright file="ArgumentActionHolderTest.cs" company="Appccelerate">
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
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.Machine.ActionHolders;
    using Xunit;

    public class ArgumentActionHolderTest
    {
        [Fact]
        public void ActionIsInvokedWithSameArgumentThatIsPassedToActionHolderExecuted()
        {
            var expected = new MyArgument();
            MyArgument value = null;
            void AnAction(MyArgument x) => value = x;

            var testee = new ArgumentActionHolder<MyArgument>(AnAction);

            testee.Execute(expected);

            value.Should().Be(expected);
        }

        [Fact]
        public void ReturnsFunctionNameForNonAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentActionHolder<MyArgument>(Action);

            var description = testee.Describe();

            description
                .Should()
                .Be("Action");
        }

        [Fact]
        public void ReturnsAnonymousForAnonymousActionWhenDescribing()
        {
            var testee = new ArgumentActionHolder<MyArgument>(a => { });

            var description = testee.Describe();

            description
                .Should()
                .Be("anonymous");
        }

        [Fact]
        public void MatchingType()
        {
            var testee = new ArgumentActionHolder<IBase>(BaseAction);

            testee.Execute(A.Fake<IBase>());
        }

        [Fact]
        public void DerivedType()
        {
            var testee = new ArgumentActionHolder<IBase>(BaseAction);

            testee.Execute(A.Fake<IDerived>());
        }

        [Fact]
        public void NonMatchingType()
        {
            var testee = new ArgumentActionHolder<IBase>(BaseAction);

            Action action = () => testee.Execute(3);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TooManyArguments()
        {
            var testee = new ArgumentActionHolder<IBase>(BaseAction);

            Action action = () => testee.Execute(new object[] { 3, 4 });

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TooFewArguments()
        {
            var testee = new ArgumentActionHolder<IBase>(BaseAction);

            Action action = () => testee.Execute(new object[] { });

            action.Should().Throw<ArgumentException>();
        }

        private static void Action(MyArgument a)
        {
        }

        private static void BaseAction(IBase b)
        {
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