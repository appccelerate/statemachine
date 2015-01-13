//-------------------------------------------------------------------------------
// <copyright file="Initializable.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine
{
    using System;

    /// <summary>
    /// A value which can be initialized.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class Initializable<T>
    {
        private T value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                this.CheckInitialized();

                return this.value;
            }

            set
            {
                this.CheckNotAlreadyInitialized();

                this.IsInitialized = true;

                this.value = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized (has a set value).
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; private set; }

        private void CheckInitialized()
        {
            if (!this.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.ValueNotInitialized);
            }
        }

        private void CheckNotAlreadyInitialized()
        {
            if (this.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.ValueAlreadyInitialized);
            }
        }
    }
}