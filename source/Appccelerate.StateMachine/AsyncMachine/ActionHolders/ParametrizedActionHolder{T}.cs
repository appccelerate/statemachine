// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParametrizedActionHolder{T}.cs" company="Appccelerate">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.AsyncMachine.ActionHolders
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class ParametrizedActionHolder<T> : IActionHolder
    {
        private readonly Func<T, Task> action;

        private readonly T parameter;

        public ParametrizedActionHolder(Func<T, Task> action, T parameter)
        {
            this.action = action;
            this.parameter = parameter;
        }

        public ParametrizedActionHolder(Action<T> action, T parameter)
        {
            this.action = argument =>
            {
                action(argument);
                return Task.CompletedTask;
            };

            this.parameter = parameter;
        }

        public async Task Execute(object argument)
        {
            await this.action(this.parameter);
        }

        public string Describe()
        {
            return this.action.GetMethodInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() ? "anonymous" : this.action.GetMethodInfo().Name;
        }
    }
}