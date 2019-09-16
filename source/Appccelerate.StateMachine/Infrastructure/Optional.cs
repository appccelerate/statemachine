//-------------------------------------------------------------------------------
// <copyright file="Optional.cs" company="Appccelerate">
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
    public class Optional<T>
    {
        private Optional()
        {
        }

        public T Value { get; private set; }

        public bool HasValue { get; private set; }

        public static Optional<T> Just(T value)
        {
            return new Optional<T>
            {
                HasValue = true,
                Value = value
            };
        }

        public static Optional<T> Nothing()
        {
            return new Optional<T>();
        }
    }
}
