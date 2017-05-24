// <copyright file="Guard.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Provides guard clauses.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Guards against a null argument.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="argument">The argument.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="argument" /> is <c>null</c>.</exception>
        /// <remarks><typeparamref name="TArgument"/> is restricted to reference types to avoid boxing of value type objects.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Distributed as a source code package.")]
        [DebuggerStepThrough]
        public static void AgainstNullArgument<TArgument>(string parameterName, [ValidatedNotNull]TArgument argument)
            where TArgument : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(parameterName, string.Format(CultureInfo.InvariantCulture, "{0} is null.", parameterName));
            }
        }

        /// <summary>
        /// Guards against a null argument if <typeparamref name="TArgument" /> can be <c>null</c>.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="argument">The argument.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="argument" /> is <c>null</c>.</exception>
        /// <remarks>
        /// Performs a type check to avoid boxing of value type objects.
        /// </remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Distributed as a source code package.")]
        [DebuggerStepThrough]
        public static void AgainstNullArgumentIfNullable<TArgument>(string parameterName, [ValidatedNotNull]TArgument argument)
        {
            if (typeof(TArgument).IsNullableType() && argument == null)
            {
                throw new ArgumentNullException(parameterName, string.Format(CultureInfo.InvariantCulture, "{0} is null.", parameterName));
            }
        }

        /// <summary>
        /// Guards against a null argument property value.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="argumentProperty">The argument property.</param>
        /// <exception cref="System.ArgumentException"><paramref name="argumentProperty" /> is <c>null</c>.</exception>
        /// <remarks><typeparamref name="TProperty"/> is restricted to reference types to avoid boxing of value type objects.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Distributed as a source code package.")]
        [DebuggerStepThrough]
        public static void AgainstNullArgumentProperty<TProperty>(string parameterName, string propertyName, [ValidatedNotNull]TProperty argumentProperty)
            where TProperty : class
        {
            if (argumentProperty == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0}.{1} is null.", parameterName, propertyName), parameterName);
            }
        }

        /// <summary>
        /// Guards against a null argument property value if <typeparamref name="TProperty"/> can be <c>null</c>.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="argumentProperty">The argument property.</param>
        /// <exception cref="System.ArgumentException"><paramref name="argumentProperty" /> is <c>null</c>.</exception>
        /// <remarks>
        /// Performs a type check to avoid boxing of value type objects.
        /// </remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Distributed as a source code package.")]
        [DebuggerStepThrough]
        public static void AgainstNullArgumentPropertyIfNullable<TProperty>(
            string parameterName, string propertyName, [ValidatedNotNull]TProperty argumentProperty)
        {
            if (typeof(TProperty).IsNullableType() && argumentProperty == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0}.{1} is null.", parameterName, propertyName), parameterName);
            }
        }

        /// <summary>
        /// Determines whether the specified type is a nullable type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is a nullable type; otherwise, <c>false</c>.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Distributed as a source code package.")]
        private static bool IsNullableType(this Type type)
        {
            return !type.GetTypeInfo().IsValueType || (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// When applied to a parameter, this attribute provides an indication to code analysis that the argument has been null checked.
        /// </summary>
        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}
