//-------------------------------------------------------------------------------
// <copyright file="ActionHoldersExceptionMessages.cs" company="Appccelerate">
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
    using System.Globalization;
    
    /// <summary>
    /// Holds all exception messages
    /// </summary>
    public static class ActionHoldersExceptionMessages
    {
        /// <summary>
        /// Cannot cast argument to action argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="action">The action.</param>
        /// <returns>error message</returns>
        public static string CannotCastArgumentToActionArgument(object argument, string action)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Cannot cast argument to match action method. Argument = {0}, Action = {1}",
                argument,
                action);
        }
    }
}