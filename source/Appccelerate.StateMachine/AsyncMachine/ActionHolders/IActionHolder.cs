//-------------------------------------------------------------------------------
// <copyright file="IActionHolder.cs" company="Appccelerate">
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
    using System.Threading.Tasks;

    /// <summary>
    /// Holds a transition action.
    /// </summary>
    public interface IActionHolder
    {
        /// <summary>
        /// Executes the transition action.
        /// </summary>
        /// <param name="argument">The state machine event argument.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Execute(object argument);

        /// <summary>
        /// Describes the action.
        /// </summary>
        /// <returns>Description of the action.</returns>
        string Describe();
    }
}