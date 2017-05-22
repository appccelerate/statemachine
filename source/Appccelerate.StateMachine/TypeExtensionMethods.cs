namespace Appccelerate.StateMachine
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensionMethods
    {
        /// <summary>
        /// Correctly formats the FullName of the specified type by taking generics into consideration.
        /// </summary>
        /// <param name="type">The type whose full name is formatted.</param>
        /// <returns>A correctly formatted full name.</returns>
        public static string FullNameToString(this Type type)
        {
            Guard.AgainstNullArgument("type", type);

            if (!type.GetTypeInfo().IsGenericType)
            {
                return type.FullName;
            }

            var partName = type.FullName.Substring(0, type.FullName.IndexOf('`'));
            var genericArgumentNames = type.GetTypeInfo().GenericTypeArguments.Select(arg => arg.FullNameToString());
            return string.Concat(partName, "<", string.Join(",", genericArgumentNames), ">");
        }
    }
}