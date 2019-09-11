//-------------------------------------------------------------------------------
// <copyright file="ArgumentLessGuardHolder.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.GuardHolders
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using static MethodNameExtractor;

    /// <summary>
    /// Holds an argument less guard.
    /// </summary>
    public class ArgumentLessGuardHolder : IGuardHolder
    {
        private readonly MethodInfo originalGuardMethodInfo;
        private readonly Func<Task<bool>> guard;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentLessGuardHolder"/> class.
        /// </summary>
        /// <param name="guard">The guard.</param>
        public ArgumentLessGuardHolder(Func<bool> guard)
        {
            this.originalGuardMethodInfo = guard.GetMethodInfo();
            this.guard = () => Task.FromResult(guard());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentLessGuardHolder"/> class.
        /// </summary>
        /// <param name="guard">The guard.</param>
        public ArgumentLessGuardHolder(Func<Task<bool>> guard)
        {
            this.originalGuardMethodInfo = guard.GetMethodInfo();
            this.guard = guard;
        }

        /// <summary>
        /// Executes the guard.
        /// </summary>
        /// <param name="argument">The state machine event argument.</param>
        /// <returns>Result of the guard execution.</returns>
        public async Task<bool> Execute(object argument)
        {
            return await this.guard().ConfigureAwait(false);
        }

        /// <summary>
        /// Describes the guard.
        /// </summary>
        /// <returns>Description of the guard.</returns>
        public string Describe()
        {
            return ExtractMethodNameOrAnonymous(this.originalGuardMethodInfo);
        }
    }
}