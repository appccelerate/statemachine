// <copyright file="Option.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Option<T>
    {
        public static Option<T> Some(T value)
            => new Option<T>(new[] { value });

        public static Option<T> None
            => new Option<T>(new T[0]);

        private readonly IEnumerable<T> values;

        private Option(
            IEnumerable<T> values)
        {
            this.values = values;
        }

        public bool IsSome
            => this.values.Any();

        public bool IsNone
            => !this.values.Any();

        private T Value
            => !this.IsSome
                ? throw new InvalidOperationException("Option does not have a value")
                : this.values.Single();

        public Option<T2> Map<T2>(Func<T, T2> f)
            => this.IsSome
                ? Option<T2>.Some(f(this.Value))
                : Option<T2>.None;

        public bool TryGetValue(out T value)
        {
            if (this.IsSome)
            {
                value = this.Value;
                return true;
            }

            value = default!;
            return false;
        }
    }
}