//-------------------------------------------------------------------------------
// <copyright file="ExceptionThrowingGuardTransitionTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine.Transitions
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.Machine;
    using Xunit;

    public class ExceptionThrowingGuardTransitionTest : TransitionTestBase
    {
        private Exception exception;

        public ExceptionThrowingGuardTransitionTest()
        {
            this.Source = Builder<States, Events>.CreateStateDefinition().Build();
            this.Target = Builder<States, Events>.CreateStateDefinition().Build();
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithStateDefinition(this.Source).Build();

            this.TransitionDefinition.Source = this.Source;
            this.TransitionDefinition.Target = this.Target;

            this.exception = new Exception();

            var guard = Builder<States, Events>.CreateGuardHolder().Throwing(this.exception).Build();
            this.TransitionDefinition.Guard = guard;
        }

        [Fact]
        public void CallsExtensionToHandleException()
        {
            var extension = A.Fake<IExtension<States, Events>>();

            this.ExtensionHost.Extension = extension;

            this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier);

            A.CallTo(() => extension.HandlingGuardException(this.StateMachineInformation, this.TransitionDefinition, this.TransitionContext, ref this.exception)).MustHaveHappened();
            A.CallTo(() => extension.HandledGuardException(this.StateMachineInformation, this.TransitionDefinition, this.TransitionContext, this.exception)).MustHaveHappened();
        }

        [Fact]
        public void ReturnsNotFiredTransitionResult()
        {
            var result = this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier);

            result.Fired.Should().BeFalse();
        }

        [Fact]
        public void NotifiesExceptionOnTransitionContext()
        {
            this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier);

            A.CallTo(() => this.TransitionContext.OnExceptionThrown(this.exception)).MustHaveHappened();
        }
    }
}