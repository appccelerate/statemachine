//-------------------------------------------------------------------------------
// <copyright file="ExceptionThrowingGuardTransitionFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.Transitions
{
    using System;
    using System.Threading.Tasks;
    using AsyncMachine;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using Xunit;

    public class ExceptionThrowingGuardTransitionFacts : TransitionFactsBase
    {
        private Exception exception;

        public ExceptionThrowingGuardTransitionFacts()
        {
            this.Source = Builder<States, Events>.CreateState().Build();
            this.Target = Builder<States, Events>.CreateState().Build();
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.Testee.Source = this.Source;
            this.Testee.Target = this.Target;

            this.exception = new Exception();

            var guard = Builder<States, Events>.CreateGuardHolder().Throwing(this.exception).Build();
            this.Testee.Guard = guard;
        }

        [Fact]
        public async Task CallsExtensionToHandleException()
        {
            var extension = A.Fake<IExtension<States, Events>>();

            this.ExtensionHost.Extension = extension;

            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => extension.HandlingGuardException(this.StateMachineInformation, this.Testee, this.TransitionContext, ref this.exception)).MustHaveHappened();
            A.CallTo(() => extension.HandledGuardException(this.StateMachineInformation, this.Testee, this.TransitionContext, this.exception)).MustHaveHappened();
        }

        [Fact]
        public async Task ReturnsNotFiredTransitionResult()
        {
            ITransitionResult<States, Events> result = await this.Testee.Fire(this.TransitionContext);

            result.Fired.Should().BeFalse();
        }

        [Fact]
        public async Task NotifiesExceptionOnTransitionContext()
        {
            await this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.TransitionContext.OnExceptionThrown(this.exception)).MustHaveHappened();
        }
    }
}