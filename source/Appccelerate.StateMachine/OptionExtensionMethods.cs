namespace Appccelerate.StateMachine
{
    using System;

    public static class OptionExtensionMethods
    {
        public static T ExtractOrNull<T>(
            this Option<T> option)
            where T : class
        {
            return option.TryGetValue(out var value)
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