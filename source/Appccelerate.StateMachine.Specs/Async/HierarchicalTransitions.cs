//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitions.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Async
{
    using System;
    using System.Globalization;
    using System.Linq;
    using AsyncMachine;
    using FluentAssertions;
    using Xbehave;

    public class HierarchicalTransitions
    {
        [Scenario]
        public void NoCommonAncestor(
            AsyncPassiveStateMachine<string, int> machine)
        {
            const string sourceState = "SourceState";
            const string parentOfSourceState = "ParentOfSourceState";
            const string siblingOfSourceState = "SiblingOfSourceState";
            const string destinationState = "DestinationState";
            const string parentOfDestinationState = "ParentOfDestinationState";
            const string siblingOfDestinationState = "SiblingOfDestinationState";
            const string grandParentOfSourceState = "GrandParentOfSourceState";
            const string grandParentOfDestinationState = "GrandParentOfDestinationState";
            const int Event = 0;

            var log = string.Empty;

            "establish a hierarchical state machine".x(async () =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<string, int>();

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(parentOfSourceState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(sourceState)
                        .WithSubState(siblingOfSourceState);

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(parentOfDestinationState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(destinationState)
                        .WithSubState(siblingOfDestinationState);

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(grandParentOfSourceState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(parentOfSourceState);

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(grandParentOfDestinationState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(parentOfDestinationState);

                stateMachineDefinitionBuilder
                    .In(sourceState)
                        .ExecuteOnExit(() => log += "exit" + sourceState)
                        .On(Event).Goto(destinationState);

                stateMachineDefinitionBuilder
                    .In(parentOfSourceState)
                        .ExecuteOnExit(() => log += "exit" + parentOfSourceState);

                stateMachineDefinitionBuilder
                    .In(destinationState)
                        .ExecuteOnEntry(() => log += "enter" + destinationState);

                stateMachineDefinitionBuilder
                    .In(parentOfDestinationState)
                        .ExecuteOnEntry(() => log += "enter" + parentOfDestinationState);

                stateMachineDefinitionBuilder
                    .In(grandParentOfSourceState)
                        .ExecuteOnExit(() => log += "exit" + grandParentOfSourceState);

                stateMachineDefinitionBuilder
                    .In(grandParentOfDestinationState)
                        .ExecuteOnEntry(() => log += "enter" + grandParentOfDestinationState);

                machine = stateMachineDefinitionBuilder
                    .WithInitialState(sourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                await machine.Start();
            });

            "when firing an event resulting in a transition without a common ancestor".x(()
                => machine.Fire(Event));

            "it should execute exit action of source state".x(() =>
                log.Should().Contain("exit" + sourceState));

            "it should execute exit action of parents of source state (recursively)".x(()
                => log
                    .Should().Contain("exit" + parentOfSourceState)
                    .And.Contain("exit" + grandParentOfSourceState));

            "it should execute entry action of parents of destination state (recursively)".x(()
                => log
                    .Should().Contain("enter" + parentOfDestinationState)
                    .And.Contain("enter" + grandParentOfDestinationState));

            "it should execute entry action of destination state".x(()
                => log.Should().Contain("enter" + destinationState));

            "it should execute actions from source upwards and then downwards to destination state".x(() =>
            {
                string[] states =
                    {
                        sourceState,
                        parentOfSourceState,
                        grandParentOfSourceState,
                        grandParentOfDestinationState,
                        parentOfDestinationState,
                        destinationState
                    };

                var statesInOrderOfAppearanceInLog = states
                    .OrderBy(s => log.IndexOf(s.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
                statesInOrderOfAppearanceInLog
                    .Should().Equal(states);
            });
        }

        [Scenario]
        public void CommonAncestor(
            AsyncPassiveStateMachine<int, int> machine)
        {
            const int commonAncestorState = 0;
            const int sourceState = 1;
            const int parentOfSourceState = 2;
            const int siblingOfSourceState = 3;
            const int destinationState = 4;
            const int parentOfDestinationState = 5;
            const int siblingOfDestinationState = 6;
            const int Event = 0;

            var commonAncestorStateLeft = false;

            "establish a hierarchical state machine".x(async () =>
            {
                var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(commonAncestorState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(parentOfSourceState)
                        .WithSubState(parentOfDestinationState);

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(parentOfSourceState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(sourceState)
                        .WithSubState(siblingOfSourceState);

                stateMachineDefinitionBuilder
                    .DefineHierarchyOn(parentOfDestinationState)
                        .WithHistoryType(HistoryType.None)
                        .WithInitialSubState(destinationState)
                        .WithSubState(siblingOfDestinationState);

                stateMachineDefinitionBuilder
                    .In(sourceState)
                        .On(Event).Goto(destinationState);

                stateMachineDefinitionBuilder
                    .In(commonAncestorState)
                        .ExecuteOnExit(() => commonAncestorStateLeft = true);

                machine = stateMachineDefinitionBuilder
                    .WithInitialState(sourceState)
                    .Build()
                    .CreatePassiveStateMachine();

                await machine.Start();
            });

            "when firing an event resulting in a transition with a common ancestor".x(()
                => machine.Fire(Event));

            "the state machine should remain inside common ancestor state".x(()
                => commonAncestorStateLeft
                    .Should().BeFalse());
        }
    }
}