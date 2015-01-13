//-------------------------------------------------------------------------------
// <copyright file="HistoryType.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine
{
    /// <summary>
    /// Defines the history behavior of a state (on re-entrance of a super state).
    /// </summary>
    public enum HistoryType
    {
        /// <summary>
        /// The state enters into its initial sub-state. The sub-state itself enters its initial sub-state and so on
        /// until the innermost nested state is reached.
        /// </summary>
        None,

        /// <summary>
        /// The state enters into its last active sub-state. The sub-state itself enters its initial sub-state and so on
        /// until the innermost nested state is reached.
        /// </summary>
        Shallow,

        /// <summary>
        /// The state enters into its last active sub-state. The sub-state itself enters into-its last active state and so on
        /// until the innermost nested state is reached.
        /// </summary>
        Deep
    }
}