//-------------------------------------------------------------------------------
// <copyright file="SuccessfulTransitionWithExecutedActionsTestBase.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using Extensions;
    using FluentAssertions;
    using StateMachine.Machine;
    using StateMachine.Machine.ActionHolders;
    using StateMachine.Machine.States;
    using StateMachine.Machine.Transitions;
    using Xunit;

    public abstract class SuccessfulTransitionWithExecutedActionsTestBase : TransitionTestBase
    {
        [Fact]
        public void ReturnsSuccessfulTransitionResult()
        {
            var result = this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            result.Should().BeSuccessfulTransitionResultWithNewState(this.Target);
        }

        [Fact]
        public void ExecutesActions()
        {
            bool executed = false;

            this.TransitionDefinition.ActionsModifiable.Add(new ArgumentLessActionHolder(() => executed = true));

            this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            executed.Should().BeTrue("actions should be executed");
        }

        [Fact]
        public void TellsExtensionsAboutExecutedTransition()
        {
            var extension = new FakeExtension();
            this.ExtensionHost.Extension = extension;

            this.Testee.Fire(this.TransitionDefinition, this.TransitionContext, this.LastActiveStateModifier, this.StateDefinitions);

            extension.Items.Should().Contain(new FakeExtension.Item(
                this.Source,
                this.Target,
                this.TransitionContext));
        }

        public class FakeExtension : InternalExtensionBase<States, Events>
        {
            private readonly List<Item> items = new List<Item>();

            public override void ExecutedTransition(
                ITransitionDefinition<States, Events> transition,
                ITransitionContext<States, Events> transitionContext)
            {
                this.items.Add(new Item(transition.Source, transition.Target, transitionContext));
            }

            public IReadOnlyCollection<Item> Items => this.items;

            public class Item
            {
                private bool Equals(Item other)
                {
                    return
                        Equals(this.Source, other.Source) &&
                        (Equals(this.Target, other.Target) || (this.Target == null && other.Target == other.Source)) && // in case of an internal-transition, this.Target (from TransitionContext) is null (wherease it would be == this.Source in case of an self-transition) therefor we check the we did not switch state in this case
                        Equals(this.TransitionContext, other.TransitionContext);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj))
                    {
                        return false;
                    }

                    if (ReferenceEquals(this, obj))
                    {
                        return true;
                    }

                    if (obj.GetType() != this.GetType())
                    {
                        return false;
                    }

                    return this.Equals((Item)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = this.Source != null ? this.Source.GetHashCode() : 0;
                        hashCode = (hashCode * 397) ^ (this.Target != null ? this.Target.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (this.TransitionContext != null ? this.TransitionContext.GetHashCode() : 0);
                        return hashCode;
                    }
                }

                public Item(
                    IStateDefinition<States, Events> source,
                    IStateDefinition<States, Events> target,
                    ITransitionContext<States, Events> transitionContext)
                {
                    this.Source = source;
                    this.Target = target;
                    this.TransitionContext = transitionContext;
                }

                public IStateDefinition<States, Events> Source { get; }

                public IStateDefinition<States, Events> Target { get; }

                public ITransitionContext<States, Events> TransitionContext { get; }
            }
        }
    }
}