//-------------------------------------------------------------------------------
// <copyright file="ArgumentLessActionHolder.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.AsyncMachine.ActionHolders
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class ArgumentLessActionHolder : IActionHolder
    {
        private readonly Func<Task> action;

        public ArgumentLessActionHolder(Func<Task> action)
        {
            this.action = action;
        }

        public ArgumentLessActionHolder(Action action)
        {
            this.action = () =>
            {
                action();
                return Task.CompletedTask;
            };
        }

        public async Task Execute(object argument)
        {
            await this.action().ConfigureAwait(false);
        }

        public string Describe()
        {
            return this.action.GetMethodInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() ? "anonymous" : this.action.GetMethodInfo().Name;
        }
    }
}