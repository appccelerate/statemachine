//-------------------------------------------------------------------------------
// <copyright file="ExceptionThrowingActionTransitionTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;
    using Appccelerate.StateMachine.Machine.ActionHolders;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ExceptionThrowingActionTransitionTest : TransitionTestBase
    {
        private Exception exception;

        public ExceptionThrowingActionTransitionTest()
        {
            this.Source = Builder<States, Events>.CreateState().Build();
            this.Target = Builder<States, Events>.CreateState().Build();
            this.TransitionContext = Builder<States, Events>.CreateTransitionContext().WithState(this.Source).Build();

            this.Testee.Source = this.Source;
            this.Testee.Target = this.Target;

            this.exception = new Exception();

            this.Testee.Actions.Add(new ArgumentLessActionHolder(() => { throw this.exception; }));
        }

        [Fact]
        public void CallsExtensionToHandleException()
        {
            var extension = A.Fake<IExtension<States, Events>>();

            this.ExtensionHost.Extension = extension;

            this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => extension.HandlingTransitionException(this.StateMachineInformation, this.Testee, this.TransitionContext, ref this.exception)).MustHaveHappened();
            A.CallTo(() => extension.HandledTransitionException(this.StateMachineInformation, this.Testee, this.TransitionContext, this.exception)).MustHaveHappened();
        }

        [Fact]
        public void ReturnsFiredTransitionResult()
        {
            ITransitionResult<States, Events> result = this.Testee.Fire(this.TransitionContext);

            result.Fired.Should().BeTrue();
        }

        [Fact]
        public void NotifiesExceptionOnTransitionContext()
        {
            this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => this.TransitionContext.OnExceptionThrown(this.exception)).MustHaveHappened();
        }
    }
}