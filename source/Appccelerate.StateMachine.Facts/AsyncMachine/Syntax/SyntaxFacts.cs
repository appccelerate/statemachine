//-------------------------------------------------------------------------------
// <copyright file="SyntaxFacts.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts.AsyncMachine.Syntax
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using StateMachine.AsyncMachine;
    using Xunit;

    public class SyntaxFacts
    {
        /// <summary>
        /// Simple check whether all possible cases can be defined with the syntax (not an actual test really).
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        [Fact]
        public void Syntax()
        {
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();

            // ReSharper disable once UnusedVariable
            Action a = () =>
                stateMachineDefinitionBuilder
                    .In(0)
                        .ExecuteOnEntry(() => { })
                        .ExecuteOnEntry((int i) => { })
                        .ExecuteOnEntry(() => Task.CompletedTask)
                        .ExecuteOnEntry((int i) => Task.CompletedTask)
                        .ExecuteOnEntryParametrized(p => { }, 4)
                        .ExecuteOnEntryParametrized(p => { }, "test")
                        .ExecuteOnEntryParametrized(p => Task.CompletedTask, 4)
                        .ExecuteOnEntryParametrized(p => Task.CompletedTask, "test")
                        .ExecuteOnExit(() => { })
                        .ExecuteOnExit((string st) => { })
                        .ExecuteOnExit(() => Task.CompletedTask)
                        .ExecuteOnExit((string st) => Task.CompletedTask)
                        .ExecuteOnExitParametrized(p => { }, 4)
                        .ExecuteOnExitParametrized(p => { }, "test")
                        .ExecuteOnExitParametrized(p => Task.CompletedTask, 4)
                        .ExecuteOnExitParametrized(p => Task.CompletedTask, "test")
                        .On(3)
                            .If(() => true).Goto(4).Execute(() => { }).Execute((int i) => { }).Execute(() => Task.CompletedTask).Execute((int i) => Task.CompletedTask)
                            .If(() => Task.FromResult(true)).Goto(4).Execute(() => { }).Execute((int i) => { }).Execute(() => Task.CompletedTask).Execute((int i) => Task.CompletedTask)
                            .If(() => true).Goto(4)
                            .If(() => true).Execute(() => { }).Execute((int i) => { }).Execute(() => Task.CompletedTask).Execute((int i) => Task.CompletedTask)
                            .If((Func<string, bool>)AGuard).Execute(() => { }).Execute((int i) => { }).Execute(() => Task.CompletedTask).Execute((int i) => Task.CompletedTask)
                            .If((Func<string, Task<bool>>)AnAsyncGuard).Execute(() => { }).Execute((int i) => { }).Execute(() => Task.CompletedTask).Execute((int i) => Task.CompletedTask)
                            .Otherwise().Goto(4).Execute(() => { }).Execute((int i) => { }).Execute(() => Task.CompletedTask).Execute((int i) => Task.CompletedTask)
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
            var stateMachineDefinitionBuilder = new StateMachineDefinitionBuilder<int, int>();
            stateMachineDefinitionBuilder
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

        private static Task<bool> AnAsyncGuard(string argument)
        {
            return Task.FromResult(true);
        }
    }
}