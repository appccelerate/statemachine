//-------------------------------------------------------------------------------
// <copyright file="SyntaxTest.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.Machine.Syntax
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using StateMachine.Machine;
    using Xunit;

    /// <summary>
    /// Tests the syntax.
    /// </summary>
    public class SyntaxTest
    {
        /// <summary>
        /// Simple check whether all possible cases can be defined with the syntax (not an actual test really).
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        [Fact]
        public void Syntax()
        {
            Action unused = () =>
                new StateMachineDefinitionBuilder<int, int>()
                    .In(0)
                        .ExecuteOnEntry(() => { })
                        .ExecuteOnEntry((int i) => { })
                        .ExecuteOnEntryParametrized(p => { }, 4)
                        .ExecuteOnEntryParametrized(p => { }, "test")
                        .ExecuteOnExit(() => { })
                        .ExecuteOnExit((string st) => { })
                        .ExecuteOnExitParametrized(p => { }, 4)
                        .ExecuteOnExitParametrized(p => { }, "test")
                        .On(3)
                            .If(() => true).Goto(4).Execute(() => { }).Execute((int i) => { })
                            .If(() => true).Goto(4)
                            .If(() => true).Execute(() => { }).Execute((int i) => { }).Execute(() => { })
                            .If<string>(AGuard).Execute(() => { }).Execute((int i) => { })
                            .Otherwise().Goto(4)
                        .On(5)
                            .If(() => true).Execute(() => { })
                            .Otherwise()
                        .On(2)
                            .If<int>(i => i != 0).Goto(7)
                            .Otherwise().Goto(7)
                        .On(1)
                            .If(() => true).Goto(7).Execute(() => { }).Execute<string>(argument => { })
                        .On(1)
                            .If(() => true).Execute(() => { })
                            .If(() => true).Execute((string argument) => { })
                            .Otherwise().Execute(() => { }).Execute((int i) => { })
                        .On(4)
                            .Goto(5).Execute(() => { }).Execute<string>(argument => { })
                        .On(5)
                            .Execute(() => { }).Execute((int i) => { })
                        .On(7)
                            .Goto(4)
                        .On(8)
                        .On(9);
        }

        [Fact]
        public void DefineHierarchySyntax()
        {
            new StateMachineDefinitionBuilder<int, int>()
                .DefineHierarchyOn(1)
                    .WithHistoryType(HistoryType.Deep)
                    .WithInitialSubState(2)
                    .WithSubState(3)
                    .WithSubState(4);
        }

        private static bool AGuard(string argument)
        {
            return true;
        }
    }
}