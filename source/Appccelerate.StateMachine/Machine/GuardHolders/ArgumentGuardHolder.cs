//-------------------------------------------------------------------------------
// <copyright file="ArgumentGuardHolder.cs" company="Appccelerate">
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
namespace Appccelerate.StateMachine.Machine.GuardHolders
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Holds a single argument guard.
    /// </summary>
    /// <typeparam name="T">Type of the argument of the guard.</typeparam>
    public class ArgumentGuardHolder<T> : IGuardHolder
    {
        private readonly Func<T, bool> guard;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGuardHolder{T}"/> class.
        /// </summary>
        /// <param name="guard">The guard.</param>
        public ArgumentGuardHolder(Func<T, bool> guard)
        {
            this.guard = guard;
        }

        /// <summary>
        /// Executes the guard.
        /// </summary>
        /// <param name="argument">The state machine event argument.</param>
        /// <returns>Result of the guard execution.</returns>
        public bool Execute(object argument)
        {
            if (argument != null && !(argument is T))
            {
                throw new ArgumentException(GuardHoldersExceptionMessages.CannotCastArgumentToGuardArgument(argument, this.Describe()));
            }

            return this.guard((T)argument);
        }

        /// <summary>
        /// Describes the guard.
        /// </summary>
        /// <returns>Description of the guard.</returns>
        public string Describe()
        {
            return this.guard.GetMethodInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() ? "anonymous" : this.guard.GetMethodInfo().Name;
        }
    }
}