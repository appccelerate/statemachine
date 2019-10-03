//-------------------------------------------------------------------------------
// <copyright file="TransitionDictionaryFacts.cs" company="Appccelerate">
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
    using System.Linq;
    using FakeItEasy;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.AsyncMachine.ActionHolders;
    using StateMachine.AsyncMachine.GuardHolders;
    using StateMachine.AsyncMachine.States;
    using StateMachine.AsyncMachine.Transitions;
    using Xunit;

    public class TransitionDictionaryFacts
    {
        [Fact]
        public void TransitionWhenTransitionIsAlreadyUsedForAnotherStateThenThrowException()
        {
            var testee = new TransitionDictionary<States, Events>(A.Fake<StateDefinition<States, Events>>());

            var transition = A.Fake<TransitionDefinition<States, Events>>();
            transition.Source = null;

            testee.Add(Events.A, transition);

            Action action = () => testee.Add(Events.B, transition);

            action
                .Should().Throw<Exception>()
                .WithMessage(ExceptionMessages.TransitionDoesAlreadyExist(transition, A.Fake<StateDefinition<States, Events>>()));
        }

        [Fact]
        public void GetTransitionsReturnsOneAddedTransition()
        {
            var testee = new TransitionDictionary<States, Events>(A.Fake<StateDefinition<States, Events>>());

            var fakeAction = A.Fake<IActionHolder>();
            var fakeGuard = A.Fake<IGuardHolder>();
            var fakeSource = A.Fake<StateDefinition<States, Events>>();
            var fakeTarget = A.Fake<StateDefinition<States, Events>>();

            var transition = new TransitionDefinition<States, Events>();
            testee.Add(Events.A, transition);

            transition.ActionsModifiable.Add(fakeAction);
            transition.Guard = fakeGuard;
            transition.Source = fakeSource;
            transition.Target = fakeTarget;

            var transitionInfos = testee.GetTransitions().ToList();
            transitionInfos.Should().HaveCount(1);

            var transitionInfo = transitionInfos.Single();
            transitionInfo.EventId.Should().Be(Events.A);
            transitionInfo.Actions.Should().ContainSingle(x => x == fakeAction);
            transitionInfo.Guard.Should().BeSameAs(fakeGuard);
            transitionInfo.Source.Should().BeSameAs(fakeSource);
            transitionInfo.Target.Should().BeSameAs(fakeTarget);
        }

        [Fact]
        public void GetTransitionsReturnsAllAddedTransitions()
        {
            var testee = new TransitionDictionary<States, Events>(A.Fake<StateDefinition<States, Events>>());

            var transitionA = A.Fake<TransitionDefinition<States, Events>>();
            var transitionB = A.Fake<TransitionDefinition<States, Events>>();
            var transitionC = A.Fake<TransitionDefinition<States, Events>>();
            testee.Add(Events.A, transitionA);
            testee.Add(Events.B, transitionB);
            testee.Add(Events.C, transitionC);

            testee.GetTransitions().Should().HaveCount(3);
        }
    }
}