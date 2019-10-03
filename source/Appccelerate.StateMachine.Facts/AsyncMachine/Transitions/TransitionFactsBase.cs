//-------------------------------------------------------------------------------
// <copyright file="TransitionFactsBase.cs" company="Appccelerate">
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
    using FakeItEasy;
    using StateMachine.AsyncMachine;
    using StateMachine.AsyncMachine.Transitions;

    public class TransitionFactsBase
    {
        public enum States
        {
            Source,
            Target,
            Some
        }

        public enum Events
        {
        }

        protected Transition<States, Events> Testee { get; }

        protected TestableExtensionHost ExtensionHost { get; }

        protected IStateMachineInformation<States, Events> StateMachineInformation { get; }

        protected IState<States, Events> Source { get; set; }

        protected IState<States, Events> Target { get; set; }

        protected ITransitionContext<States, Events> TransitionContext { get; set; }

        protected TransitionFactsBase()
        {
            this.StateMachineInformation = A.Fake<IStateMachineInformation<States, Events>>();
            this.ExtensionHost = new TestableExtensionHost();

            this.Testee = new Transition<States, Events>(this.StateMachineInformation, this.ExtensionHost);
        }

        protected class TestableExtensionHost : IExtensionHost<States, Events>
        {
            public IExtension<States, Events> Extension { private get; set; }

            public async Task ForEach(Func<IExtension<States, Events>, Task> action)
            {
                if (this.Extension != null)
                {
                    await action(this.Extension);
                }
            }
        }
    }
}