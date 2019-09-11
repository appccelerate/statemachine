//-------------------------------------------------------------------------------
// <copyright file="ArgumentActionHolder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.ActionHolders
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using static MethodNameExtractor;

    public class ArgumentActionHolder<T> : IActionHolder
    {
        private readonly MethodInfo originalActionMethodInfo;
        private readonly Func<T, Task> action;

        public ArgumentActionHolder(Func<T, Task> action)
        {
            this.originalActionMethodInfo = action.GetMethodInfo();
            this.action = action;
        }

        public ArgumentActionHolder(Action<T> action)
        {
            this.originalActionMethodInfo = action.GetMethodInfo();
            this.action = argument =>
            {
                action(argument);
                return TaskEx.Completed;
            };
        }

        public async Task Execute(object argument)
        {
            T castArgument = default(T);

            if (argument != Missing.Value && argument != null && !(argument is T))
            {
                throw new ArgumentException(ActionHoldersExceptionMessages.CannotCastArgumentToActionArgument(argument, this.Describe()));
            }

            if (argument != Missing.Value)
            {
                castArgument = (T)argument;
            }

            await this.action(castArgument).ConfigureAwait(false);
        }

        public string Describe()
        {
            return ExtractMethodNameOrAnonymous(this.originalActionMethodInfo);
        }
    }
}