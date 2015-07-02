//-------------------------------------------------------------------------------
// <copyright file="TransitionsTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests transition behavior.
    /// </summary>
    public class TransitionsTest
    {
        /// <summary>
        /// Object under test.
        /// </summary>
        private readonly StateMachine<StateMachine.States, StateMachine.Events> testee;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionsTest"/> class.
        /// </summary>
        public TransitionsTest()
        {
            this.testee = new StateMachine<StateMachine.States, StateMachine.Events>();
        }

        /// <summary>
        /// When no transition for the fired event can be found in the entire
        /// hierarchy up from the current state then the transition declined event is fired and 
        /// the state machine remains in its current state.
        /// </summary>
        [Fact]
        public void MissingTransition()
        {
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B);

            bool declined = false;

            this.testee.TransitionDeclined += (sender, e) =>
                                                  {
                                                      declined = true;
                                                  };

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.C);

            declined.Should().BeTrue("Declined event was not fired");
            this.testee.CurrentStateId.Should().Be(StateMachine.States.A);
        }

        /// <summary>
        /// Actions on transitions are performed and the event arguments are passed to them.
        /// </summary>
        [Fact]
        public void ExecuteActions()
        {
            const int EventArgument = 17;

            int? action1Argument = null;
            int? action2Argument = null;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B)
                    .Execute<int>(argument => { action1Argument = argument; })
                    .Execute((int argument) => { action2Argument = argument; });

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, EventArgument);

            action1Argument.Should().Be(EventArgument);
            action2Argument.Should().Be(EventArgument);
        }

        [Fact]
        public void ExecutesActions_WhenActionsWithAndWithoutArgumentAreDefined()
        {
            const int EventArgument = 17;

            bool action1Executed = false;
            bool action2Executed = false;
            
            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Goto(StateMachine.States.B)
                    .Execute<int>(argument => { action1Executed = true; })
                    .Execute(() => { action2Executed = true; });

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, EventArgument);

            action1Executed.Should().BeTrue("action with argument should be executed");
            action2Executed.Should().BeTrue("action without argument should be executed");
        }

        /// <summary>
        /// Internal transitions can be executed 
        /// (internal transition = transition that remains in the same state and does not execute exit
        /// and entry actions.
        /// </summary>
        [Fact]
        public void InternalTransition()
        {
            bool executed = false;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.A).Execute(() => executed = true);
            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.A);

            executed.Should().BeTrue("internal transition was not executed.");
            this.testee.CurrentStateId.Should().Be(StateMachine.States.A);
        }

        [Fact]
        public void ActionsWithoutArguments()
        {
            bool executed = false;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Execute(() => executed = true);

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B);

            executed.Should().BeTrue();
        }

        [Fact]
        public void ActionsWithOneArgument()
        {
            const int ExpectedValue = 1;
            int value = 0;

            this.testee.In(StateMachine.States.A)
                .On(StateMachine.Events.B).Execute<int>(v => value = v);

            this.testee.Initialize(StateMachine.States.A);
            this.testee.EnterInitialState();

            this.testee.Fire(StateMachine.Events.B, ExpectedValue);

            value.Should().Be(ExpectedValue);
        }
    }
}