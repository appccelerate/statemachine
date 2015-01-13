//-------------------------------------------------------------------------------
// <copyright file="SuccessfulTransitionWithExecutedActionsTestBase.cs" company="Appccelerate">
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
    using Appccelerate.StateMachine.Machine.ActionHolders;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public abstract class SuccessfulTransitionWithExecutedActionsTestBase : TransitionTestBase
    {
        [Fact]
        public void ReturnsSuccessfulTransitionResult()
        {
            ITransitionResult<States, Events> result = this.Testee.Fire(this.TransitionContext);

            result.Should().BeSuccessfulTransitionResultWithNewState(this.Target);
        }

        [Fact]
        public void ExecutesActions()
        {
            bool executed = false;

            this.Testee.Actions.Add(new ArgumentLessActionHolder(() => executed = true));

            this.Testee.Fire(this.TransitionContext);

            executed.Should().BeTrue("actions should be executed");
        }

        [Fact]
        public void TellsExtensionsAboutExecutedTransition()
        {
            var extension = A.Fake<IExtension<States, Events>>();
            this.ExtensionHost.Extension = extension;

            this.Testee.Fire(this.TransitionContext);

            A.CallTo(() => extension.ExecutedTransition(
                this.StateMachineInformation,
                A<ITransition<States, Events>>.That.Matches(t => t.Source == this.Source && t.Target == this.Target),
                this.TransitionContext));
        }
    }
}