//-------------------------------------------------------------------------------
// <copyright file="ArgumentActionHolderFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.ActionHolders
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public static class ArgumentActionHolderFacts
    {
        public class SyncUsage
        {
            private readonly ArgumentActionHolder<MyArgument> testee;
            private Action<MyArgument> action;

            public SyncUsage()
            {
                this.testee = new ArgumentActionHolder<MyArgument>(s => this.action(s));
            }

            [Fact]
            public async Task NullArgumentsArePassedToAction()
            {
                MyArgument value = new MyArgument();

                this.action = s => value = s;

                await this.testee.Execute(null);

                value.Should().Be(null);
            }
        }

        public class AsyncUsage
        {
            private readonly ArgumentActionHolder<MyArgument> testee;
            private Action<MyArgument> action;

            public AsyncUsage()
            {
                this.testee = new ArgumentActionHolder<MyArgument>(s =>
                {
                    this.action(s);
                    return Task.CompletedTask;
                });
            }

            [Fact]
            public async Task NullArgumentsArePassedToAction()
            {
                MyArgument value = new MyArgument();

                this.action = s => value = s;

                await this.testee.Execute(null);

                value.Should().Be(null);
            }
        }

        public class ArgumentCasting
        {
            public interface IBase
            {
            }

            // ReSharper disable once MemberCanBePrivate.Global
            public interface IDerived : IBase
            {
            }

            [Fact]
            public async Task MatchingType()
            {
                var testee = new ArgumentActionHolder<IBase>(BaseAction);

                await testee.Execute(A.Fake<IBase>());
            }

            [Fact]
            public async Task DerivedType()
            {
                var testee = new ArgumentActionHolder<IBase>(BaseAction);

                await testee.Execute(A.Fake<IDerived>());
            }

            [Fact]
            public void NonMatchingType()
            {
                var testee = new ArgumentActionHolder<IBase>(BaseAction);

                Func<Task> action = async () => await testee.Execute(3);

                action.ShouldThrow<ArgumentException>();
            }

            [Fact]
            public void TooManyArguments()
            {
                var testee = new ArgumentActionHolder<IBase>(BaseAction);

                Func<Task> action = async () => await testee.Execute(new object[] { 3, 4 });

                action.ShouldThrow<ArgumentException>();
            }

            [Fact]
            public void TooFewArguments()
            {
                var testee = new ArgumentActionHolder<IBase>(BaseAction);

                Func<Task> action = async () => await testee.Execute(new object[] { });

                action.ShouldThrow<ArgumentException>();
            }

            private static Task BaseAction(IBase b)
            {
                return Task.CompletedTask;
            }
        }

        public class MyArgument
        {
        }
    }
}