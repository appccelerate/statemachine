//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitions.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine
{
    using System;
    using System.Globalization;
    using System.Linq;

    using FluentAssertions;

    using Xbehave;

    public class HierarchicalTransitions
    {
        [Scenario]
        public void NoCommonAncestor(
            PassiveStateMachine<string, int> machine)
        {
            const string SourceState = "SourceState";
            const string ParentOfSourceState = "ParentOfSourceState";
            const string SiblingOfSourceState = "SiblingOfSourceState";
            const string DestinationState = "DestinationState";
            const string ParentOfDestinationState = "ParentOfDestinationState";
            const string SiblingOfDestinationState = "SiblingOfDestinationState";
            const string GrandParentOfSourceState = "GrandParentOfSourceState";
            const string GrandParentOfDestinationState = "GrandParentOfDestinationState";
            const int Event = 0;

            string log = string.Empty;

            "establish a hierarchical state machine"._(() =>
            {
                machine = new PassiveStateMachine<string, int>();

                machine.DefineHierarchyOn(ParentOfSourceState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(SourceState)
                    .WithSubState(SiblingOfSourceState);

                machine.DefineHierarchyOn(ParentOfDestinationState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(DestinationState)
                    .WithSubState(SiblingOfDestinationState);

                machine.DefineHierarchyOn(GrandParentOfSourceState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(ParentOfSourceState);

                machine.DefineHierarchyOn(GrandParentOfDestinationState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(ParentOfDestinationState);

                machine.In(SourceState)
                    .ExecuteOnExit(() => log += "exit" + SourceState)
                    .On(Event).Goto(DestinationState);

                machine.In(ParentOfSourceState)
                    .ExecuteOnExit(() => log += "exit" + ParentOfSourceState);

                machine.In(DestinationState)
                    .ExecuteOnEntry(() => log += "enter" + DestinationState);

                machine.In(ParentOfDestinationState)
                    .ExecuteOnEntry(() => log += "enter" + ParentOfDestinationState);

                machine.In(GrandParentOfSourceState)
                    .ExecuteOnExit(() => log += "exit" + GrandParentOfSourceState);

                machine.In(GrandParentOfDestinationState)
                    .ExecuteOnEntry(() => log += "enter" + GrandParentOfDestinationState);

                machine.Initialize(SourceState);
                machine.Start();
            });

            "when firing an event resulting in a transition without a common ancestor"._(() =>
                machine.Fire(Event));

            "it should execute exit action of source state"._(() =>
                log.Should().Contain("exit" + SourceState));

            "it should execute exit action of parents of source state (recursively)"._(() =>
                log
                    .Should().Contain("exit" + ParentOfSourceState)
                    .And.Contain("exit" + GrandParentOfSourceState));

            "it should execute entry action of parents of destination state (recursively)"._(() =>
                log.Should().Contain("enter" + ParentOfDestinationState)
                .And.Contain("enter" + GrandParentOfDestinationState));

            "it should execute entry action of destination state"._(() =>
                log.Should().Contain("enter" + DestinationState));

            "it should execute actions from source upwards and then downwards to destination state"._(() =>
            {
                string[] states =
                    {
                        SourceState,
                        ParentOfSourceState,
                        GrandParentOfSourceState,
                        GrandParentOfDestinationState,
                        ParentOfDestinationState,
                        DestinationState
                    };

                var statesInOrderOfAppearanceInLog = states
                    .OrderBy(s => log.IndexOf(s.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
                statesInOrderOfAppearanceInLog
                    .Should().Equal(states);
            });
        }

        [Scenario]
        public void CommonAncestor(
            PassiveStateMachine<int, int> machine)
        {
            const int CommonAncestorState = 0;
            const int SourceState = 1;
            const int ParentOfSourceState = 2;
            const int SiblingOfSourceState = 3;
            const int DestinationState = 4;
            const int ParentOfDestinationState = 5;
            const int SiblingOfDestinationState = 6;
            const int Event = 0;
            
            bool commonAncestorStateLeft = false;
            
            "establish a hierarchical state machine"._(() =>
            {
                machine = new PassiveStateMachine<int, int>();

                machine.DefineHierarchyOn(CommonAncestorState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(ParentOfSourceState)
                    .WithSubState(ParentOfDestinationState);

                machine.DefineHierarchyOn(ParentOfSourceState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(SourceState)
                    .WithSubState(SiblingOfSourceState);

                machine.DefineHierarchyOn(ParentOfDestinationState)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(DestinationState)
                    .WithSubState(SiblingOfDestinationState);

                machine.In(SourceState)
                    .On(Event).Goto(DestinationState);

                machine.In(CommonAncestorState)
                    .ExecuteOnExit(() => commonAncestorStateLeft = true);

                machine.Initialize(SourceState);
                machine.Start();
            });

            "when firing an event resulting in a transition with a common ancestor"._(() =>
                machine.Fire(Event));

            "the state machine should remain inside common ancestor state"._(() =>
                commonAncestorStateLeft
                    .Should().BeFalse());
        }
    }
}