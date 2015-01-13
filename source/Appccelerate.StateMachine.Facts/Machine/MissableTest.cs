//-------------------------------------------------------------------------------
// <copyright file="MissableTest.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

namespace Appccelerate.StateMachine.Machine
{
    using System;

    using FluentAssertions;

    using Xunit;

    public class MissableTest
    {
        private const string Value = "value";

        [Fact]
        public void ReturnsMissing_WhenNoValueIsSet()
        {
            var testee = new Missable<string>();

            testee.IsMissing.Should().BeTrue();
        }

        [Fact]
        public void ReturnsNotMissing_WhenValueIsSetInConstructor()
        {
            var testee = new Missable<string>(Value);

            testee.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void ReturnsValue_WhenValueIsSetInConstructor()
        {
            var testee = new Missable<string>(Value);

            testee.Value
                .Should().Be(Value);
        }

        [Fact]
        public void ThrowsExceptionOnAccessingValue_WhenValueIsNotSet()
        {
            var testee = new Missable<string>();

            // ReSharper disable once UnusedVariable
            Action action = () => { string v = testee.Value; };

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("*missing*");
        }
    }
}