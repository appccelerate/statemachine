//-------------------------------------------------------------------------------
// <copyright file="StateMachineReportGeneratorTest.cs" company="Appccelerate">
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
namespace Appccelerate.StateMachine.Facts.AsyncMachine.Reports
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using StateMachine.AsyncMachine;
    using StateMachine.AsyncMachine.Reports;
    using Xunit;

    public class StateMachineReportGeneratorTest
    {
        public static IEnumerable<object[]> StateMachineInstantiationProvider =>
            new List<object[]>
            {
                new object[] { "AsyncPassiveStateMachine", new Func<string, StateMachineDefinition<States, Events>, IAsyncStateMachine<States, Events>>((name, smd) => smd.CreatePassiveStateMachine(name)) },
                new object[] { "AsyncActiveStateMachine", new Func<string, StateMachineDefinition<States, Events>, IAsyncStateMachine<States, Events>>((name, smd) => smd.CreateActiveStateMachine(name)) }
            };

        [Theory]
        [MemberData(nameof(StateMachineInstantiationProvider))]
        public void Report(string dummyName, Func<string, StateMachineDefinition<States, Events>, IAsyncStateMachine<States, Events>> createStateMachine)
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<States, Events>();
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.B)
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState(States.B1)
                    .WithSubState(States.B2);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.C)
                    .WithHistoryType(HistoryType.Shallow)
                    .WithInitialSubState(States.C1)
                    .WithSubState(States.C2);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.C1)
                    .WithHistoryType(HistoryType.Shallow)
                    .WithInitialSubState(States.C1A)
                    .WithSubState(States.C1B);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.D)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(States.D1)
                    .WithSubState(States.D2);
            stateMachineDefinitionBuilder
                .DefineHierarchyOn(States.D1)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(States.D1A)
                    .WithSubState(States.D1B);
            stateMachineDefinitionBuilder
                .In(States.A)
                    .ExecuteOnEntry(EnterA)
                    .ExecuteOnExit(ExitA)
                    .On(Events.A)
                    .On(Events.B).Goto(States.B)
                    .On(Events.C).If(AlwaysTrue).Goto(States.C1)
                    .On(Events.C).If(AlwaysFalse).Goto(States.C2);
            stateMachineDefinitionBuilder
                .In(States.B)
                    .On(Events.A).Goto(States.A).Execute(Action);
            stateMachineDefinitionBuilder
                .In(States.B1)
                    .On(Events.B2).Goto(States.B1);
            stateMachineDefinitionBuilder
                .In(States.B2)
                    .On(Events.B1).Goto(States.B2);
            var stateMachineDefinition = stateMachineDefinitionBuilder
                .WithInitialState(States.A)
                .Build();

            var stateMachine = createStateMachine("Test Machine", stateMachineDefinition);

            var testee = new StateMachineReportGenerator<States, Events>();
            stateMachine.Report(testee);

            var actualReport = testee.Result;

            const string ExpectedReport =
@"Test Machine: initial state = A
    B: initial state = B1 history type = None
        entry action: 
        exit action: 
        A -> A actions: Action guard: 
        B1: initial state = None history type = None
            entry action: 
            exit action: 
            B2 -> B1 actions:  guard: 
        B2: initial state = None history type = None
            entry action: 
            exit action: 
            B1 -> B2 actions:  guard: 
    C: initial state = C1 history type = Shallow
        entry action: 
        exit action: 
        C1: initial state = C1A history type = Shallow
            entry action: 
            exit action: 
            C1A: initial state = None history type = None
                entry action: 
                exit action: 
            C1B: initial state = None history type = None
                entry action: 
                exit action: 
        C2: initial state = None history type = None
            entry action: 
            exit action: 
    D: initial state = D1 history type = Deep
        entry action: 
        exit action: 
        D1: initial state = D1A history type = Deep
            entry action: 
            exit action: 
            D1A: initial state = None history type = None
                entry action: 
                exit action: 
            D1B: initial state = None history type = None
                entry action: 
                exit action: 
        D2: initial state = None history type = None
            entry action: 
            exit action: 
    A: initial state = None history type = None
        entry action: EnterA
        exit action: ExitA
        A -> internal actions:  guard: 
        B -> B actions:  guard: 
        C -> C1 actions:  guard: AlwaysTrue
        C -> C2 actions:  guard: AlwaysFalse
";
            actualReport
                .IgnoringNewlines()
                .Should()
                .Be(
                    ExpectedReport
                        .IgnoringNewlines());
        }

        private static void EnterA()
        {
        }

        private static void ExitA()
        {
        }

        private static void Action()
        {
        }

        private static bool AlwaysTrue()
        {
            return true;
        }

        private static bool AlwaysFalse()
        {
            return false;
        }
    }
}