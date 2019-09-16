//-------------------------------------------------------------------------------
// <copyright file="TransitionTestBase.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine.Transitions
{
    using System;
    using FakeItEasy;
    using StateMachine.Machine;
    using StateMachine.Machine.States;
    using StateMachine.Machine.Transitions;

    public class TransitionTestBase
    {
        public enum States
        {
        }

        public enum Events
        {
        }

        protected TransitionDefinition<States, Events> TransitionDefinition { get; }

        protected TransitionLogic<States, Events> Testee { get; }

        protected IStateLogic<States, Events> StateLogic { get; }

        protected ILastActiveStateModifier<States> LastActiveStateModifier { get; }

        protected TestableExtensionHost ExtensionHost { get; }

        protected IStateDefinitionDictionary<States, Events> StateDefinitions { get; }

        protected IStateDefinition<States, Events> Source { get; set; }

        protected IStateDefinition<States, Events> Target { get; set; }

        protected ITransitionContext<States, Events> TransitionContext { get; set; }

        public TransitionTestBase()
        {
            this.StateLogic = A.Fake<IStateLogic<States, Events>>();
            this.LastActiveStateModifier = A.Fake<ILastActiveStateModifier<States>>();
            this.StateDefinitions = A.Fake<IStateDefinitionDictionary<States, Events>>();
            this.ExtensionHost = new TestableExtensionHost();
            this.TransitionDefinition = new TransitionDefinition<States, Events>();

            this.Testee = new TransitionLogic<States, Events>(this.ExtensionHost);
            this.Testee.SetStateLogic(this.StateLogic);
        }

        public class TestableExtensionHost : IExtensionHost<States, Events>
        {
            public IExtensionInternal<States, Events> Extension { private get; set; }

            public void ForEach(Action<IExtensionInternal<States, Events>> action)
            {
                if (this.Extension != null)
                {
                    action(this.Extension);
                }
            }
        }
    }
}