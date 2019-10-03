//-------------------------------------------------------------------------------
// <copyright file="InitializableTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Infrastructure
{
    using System;
    using FluentAssertions;
    using StateMachine.Infrastructure;
    using StateMachine.Machine;
    using Xunit;

    public class InitializableTest
    {
        [Fact]
        public void IsInitialized()
        {
            Initializable<SomeClass>
                .Initialized(new SomeClass())
                .IsInitialized
                .Should()
                .BeTrue();

            Initializable<SomeClass>
                .UnInitialized()
                .IsInitialized
                .Should()
                .BeFalse();
        }

        [Fact]
        public void ExtractOr()
        {
            Initializable<string>
                .Initialized("A")
                .ExtractOr("B")
                .Should()
                .Be("A");

            Initializable<string>
                .UnInitialized()
                .ExtractOr("B")
                .Should()
                .Be("B");
        }

        [Fact]
        public void ExtractOrThrow()
        {
            Initializable<string>
                .Initialized("A")
                .ExtractOrThrow()
                .Should()
                .Be("A");

            Initializable<string>
                .UnInitialized()
                .Invoking(x => x.ExtractOrThrow())
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(ExceptionMessages.ValueNotInitialized);
        }

        [Fact]
        public void Map()
        {
            Initializable<SomeClass>
                .Initialized(new SomeClass { SomeValue = "A" })
                .Map(x => x.SomeValue)
                .Should()
                .BeEquivalentTo(Initializable<string>.Initialized("A"));

            Initializable<SomeClass>
                .UnInitialized()
                .Map(x => x.SomeValue)
                .Should()
                .BeEquivalentTo(Initializable<string>.UnInitialized());
        }

        private class SomeClass
        {
            public string SomeValue { get; set; }
        }
    }
}
