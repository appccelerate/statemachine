//-------------------------------------------------------------------------------
// <copyright file="ArgumentActionHolder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.ActionHolders
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class ArgumentActionHolder<T> : IActionHolder
    {
        private readonly Action<T> action;

        public ArgumentActionHolder(Action<T> action)
        {
            this.action = action;
        }

        public void Execute(object argument)
        {
            T castArgument = default(T);

            if (argument != Missing.Value && !(argument is T))
            {
                throw new ArgumentException(ActionHoldersExceptionMessages.CannotCastArgumentToActionArgument(argument, this.Describe()));
            }

            if (argument != Missing.Value)
            {
                castArgument = (T)argument;
            }

            this.action(castArgument);
        }

        public string Describe()
        {
            return this.action.GetMethodInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() ? "anonymous" : this.action.GetMethodInfo().Name;
        }
    }
}