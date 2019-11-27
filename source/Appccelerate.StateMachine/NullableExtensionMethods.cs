namespace Appccelerate.StateMachine
{
    using System;

    public static class NullableExtensionMethods
    {
        public static TResult? Map<T, TResult>(
            this T? value,
            Func<T, TResult> f)
            where T : class
            where TResult : class
        {
            return value != null ? f(value) : default;
        }
    }
}