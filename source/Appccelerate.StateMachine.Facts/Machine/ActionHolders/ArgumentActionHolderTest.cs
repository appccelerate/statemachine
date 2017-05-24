//-------------------------------------------------------------------------------
// <copyright file="ArgumentActionHolderTest.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Machine.ActionHolders
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class ArgumentActionHolderTest
    {
        private readonly ArgumentActionHolder<MyArgument> testee;
        private Action<MyArgument> action;

        public ArgumentActionHolderTest()
        {
            this.testee = new ArgumentActionHolder<MyArgument>(s => this.action(s));
        }

        [Fact]
        public void NullArgumentsArePassedToAction()
        {
            MyArgument value = new MyArgument();

            this.action = s => value = s;

            this.testee.Execute(null);

            value.Should().Be(null);
        }

        public class MyArgument
        {
        }
    }
}