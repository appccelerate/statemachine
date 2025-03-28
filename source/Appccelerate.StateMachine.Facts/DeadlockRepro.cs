// <copyright file="DeadlockRepro.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Facts
{
    using System.Threading.Tasks;
    using Appccelerate.StateMachine.AsyncMachine;
    using Xunit;

    // https://github.com/appccelerate/statemachine/issues/40
    public class DeadlockRepro
    {
        private AsyncActiveStateMachine<int, int> machine;

        private TaskCompletionSource<int> myTcs = new TaskCompletionSource<int>();

        public DeadlockRepro()
        {
            var builder = new StateMachineDefinitionBuilder<int, int>();

            builder
                .In(0).On(1).Execute(() => myTcs.SetResult(0));

            machine = builder
                .WithInitialState(0)
                .Build()
                .CreateActiveStateMachine();
        }

        [Fact]
        public async Task DoesNotDeadlock()
        {
            await machine.Fire(1);
            await machine.Start();
            await myTcs.Task.ConfigureAwait(false);
            await machine.Stop();
        }
    }
}