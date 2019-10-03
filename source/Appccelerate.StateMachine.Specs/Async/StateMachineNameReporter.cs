//-------------------------------------------------------------------------------
// <copyright file="StateMachineNameReporter.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using AsyncMachine;
    using AsyncMachine.States;

    public class StateMachineNameReporter : IStateMachineReport<string, int>
    {
        public string StateMachineName { get; private set; }

        public void Report(string name, IEnumerable<IStateDefinition<string, int>> states, string initialStateId)
        {
            this.StateMachineName = name;
        }
    }
}