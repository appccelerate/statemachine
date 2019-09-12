//-------------------------------------------------------------------------------
// <copyright file="Initializable.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Infrastructure
{
    using System;
    using Machine;

    /// <summary>
    /// A value which can either be initialized or not.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class Initializable<T> : IInitializable<T>
    {
        private T value;

        /// <summary>
        /// Gets a value indicating whether this instance is initialized (has a set value).
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; private set; }

        private Initializable()
        {
        }

        public static Initializable<T> UnInitialized()
        {
            return new Initializable<T>();
        }

        public static Initializable<T> Initialized(T t)
        {
            return new Initializable<T>
            {
                value = t,
                IsInitialized = true
            };
        }

        public Initializable<TResult> Map<TResult>(Func<T, TResult> func)
        {
            return
                this.IsInitialized
                    ? Initializable<TResult>.Initialized(func(this.value))
                    : Initializable<TResult>.UnInitialized();
        }

        public T ExtractOr(T valueIfNotInitialized)
        {
            return
                this.IsInitialized
                    ? this.value
                    : valueIfNotInitialized;
        }

        public T ExtractOrThrow()
        {
            this.CheckInitialized();
            return this.value;
        }

        private void CheckInitialized()
        {
            if (!this.IsInitialized)
            {
                throw new InvalidOperationException(ExceptionMessages.ValueNotInitialized);
            }
        }
    }
}