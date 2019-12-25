// <copyright file="OptionExtensionMethods.cs" company="Appccelerate">
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

    public static class OptionExtensionMethods
    {
        public static T ExtractOr<T>(
            this Option<T> option,
            T v)
        {
            return option != null && option.TryGetValue(out var value)
                ? value
                : v;
        }

        public static T? ExtractOrNull<T>(
            this Option<T> option)
            where T : class
        {
            return option != null && option.TryGetValue(out var value)
                ? value
                : null;
        }

        public static T ExtractOrThrow<T>(
            this Option<T> option)
        {
            return ExtractOrThrow(
                option,
                null);
        }

        public static T ExtractOrThrow<T>(
            this Option<T> option,
            Func<Exception>? exception)
        {
            if (option.TryGetValue(out var value))
            {
                return value;
            }

            throw exception?.Invoke() ?? new InvalidOperationException("Option has no value.");
        }
    }
}